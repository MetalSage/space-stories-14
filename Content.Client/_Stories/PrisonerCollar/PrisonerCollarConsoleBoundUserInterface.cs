using Content.Client.Eye;
using Content.Shared._Stories.PrisonerCollar;
using Robust.Client.UserInterface;

namespace Content.Client._Stories.PrisonerCollar;

public sealed partial class PrisonerCollarConsoleBoundUserInterface : BoundUserInterface
{
    private EntityUid? _currentActiveEyeEntity;
    [Dependency] private EyeLerpingSystem _eyeLerpingSystem = default!;

    private PrisonerCollarConsoleWindow? _window;

    public PrisonerCollarConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<PrisonerCollarConsoleWindow>();

        _window.OnDetonate += collar => SendMessage(new PrisonerCollarConsoleDetonateMessage(collar));
        _window.OnShock += collar => SendMessage(new PrisonerCollarConsoleShockMessage(collar));
        _window.OnUnlock += collar => SendMessage(new PrisonerCollarConsoleUnlockMessage(collar));
        _window.OnUnlink += collar => SendMessage(new PrisonerCollarConsoleUnlinkMessage(collar));
        _window.OnScanPair += () => SendMessage(new PrisonerCollarConsoleScanPairMessage());

        _window.OnCollarSelected += collar => SendMessage(new PrisonerCollarConsoleSelectCameraMessage(collar));

        _window.OnCameraSelected += targetUid =>
        {
            SetCameraEyeTarget(targetUid);
        };

        _window.OpenCentered();
    }

    private void SetCameraEyeTarget(EntityUid? targetUid)
    {
        if (_currentActiveEyeEntity == targetUid)
            return;

        if (_currentActiveEyeEntity.HasValue)
        {
            _eyeLerpingSystem.RemoveEye(_currentActiveEyeEntity.Value);
            _currentActiveEyeEntity = null;
        }

        if (targetUid.HasValue && EntMan.EntityExists(targetUid.Value))
        {
            _currentActiveEyeEntity = targetUid.Value;
            _eyeLerpingSystem.AddEye(targetUid.Value);

            if (EntMan.TryGetComponent<EyeComponent>(targetUid.Value, out var eyeComp))
            {
                _window?.SetCameraView(eyeComp.Eye);
                return;
            }
        }

        _window?.SetCameraView(null);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is PrisonerCollarConsoleBoundUserInterfaceState castState)
            _window?.UpdateState(castState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_currentActiveEyeEntity.HasValue)
        {
            _eyeLerpingSystem.RemoveEye(_currentActiveEyeEntity.Value);
            _currentActiveEyeEntity = null;
        }

        _window?.Dispose();
    }
}
