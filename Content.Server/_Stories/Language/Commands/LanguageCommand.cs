using System.Linq;
using Content.Server._Stories.Language.Systems;
using Content.Server.Administration;
using Content.Shared._Stories.Language.Components;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Server._Stories.Language.Commands;

[ToolshedCommand, AdminCommand(AdminFlags.VarEdit)]
public sealed class LanguageCommand : ToolshedCommand
{
    private LanguageSystem? _language;

    [CommandImplementation("add")]
    public EntityUid Add(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] EntityUid ent,
        [CommandArgument] ProtoId<LanguagePrototype> language,
        [CommandArgument] bool canSpeak,
        [CommandArgument] bool canUnderstand)
    {
        if (!EntityManager.HasComponent<LanguageComponent>(ent))
        {
            ctx.WriteLine("Cannot add language to entity without a language comp!");
            return ent;
        }

        _language ??= GetSys<LanguageSystem>();

        _language.AddLanguage(ent, language, canSpeak, canUnderstand);

        return ent;
    }

    [CommandImplementation("remove")]
    public EntityUid Remove(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] EntityUid ent,
        [CommandArgument] ProtoId<LanguagePrototype> language,
        [CommandArgument] bool removeSpeaking,
        [CommandArgument] bool removeUnderstanding)
    {
        if (!EntityManager.HasComponent<LanguageComponent>(ent))
        {
            ctx.WriteLine("Cannot remove language from entity without a language comp!");
            return ent;
        }

        _language ??= GetSys<LanguageSystem>();

        _language.RemoveLanguage(ent, language, removeSpeaking, removeUnderstanding);

        return ent;
    }

    [CommandImplementation("reset")]
    public EntityUid Reset(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] EntityUid ent,
        [CommandArgument] ProtoId<LanguagePrototype> language)
    {
        if (!EntityManager.TryGetComponent<LanguageComponent>(ent, out var languages))
        {
            ctx.WriteLine("Cannot reset languages from entity without a language comp!");
            return ent;
        }

        _language ??= GetSys<LanguageSystem>();

        HashSet<ProtoId<LanguagePrototype>> langs = new();

        langs.UnionWith(languages.UnderstoodLanguages);
        langs.UnionWith(languages.SpokenLanguages);

        langs.Remove(language);

        _language.AddLanguage(ent, language);

        foreach (var known in langs)
        {
            _language.RemoveLanguage(ent, known);
        }

        return ent;
    }

    [CommandImplementation("add")]
    public IEnumerable<EntityUid> Add(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] IEnumerable<EntityUid> ents,
        [CommandArgument] ProtoId<LanguagePrototype> language,
        [CommandArgument] bool canSpeak,
        [CommandArgument] bool canUnderstand)
    {
        return ents.Select(ent => Add(ctx, ent, language, canSpeak, canUnderstand));
    }

    [CommandImplementation("remove")]
    public IEnumerable<EntityUid> Remove(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] IEnumerable<EntityUid> ents,
        [CommandArgument] ProtoId<LanguagePrototype> language,
        [CommandArgument] bool removeSpeaking,
        [CommandArgument] bool removeUnderstanding)
    {
        return ents.Select(ent => Remove(ctx, ent, language, removeSpeaking, removeUnderstanding));
    }
}
