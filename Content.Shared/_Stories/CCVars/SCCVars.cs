﻿using Robust.Shared.Configuration;

namespace Content.Shared._Stories.SCCVars;

/// <summary>
///     Stories modules console variables
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class SCCVars
{
    /**
     * TTS (Text-To-Speech)
     */

    /// <summary>
    /// URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabled =
        CVarDef.Create("tts.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Whether the TTS system is enabled on the client.
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabledClient =
        CVarDef.Create("tts.enabled_client", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiUrl =
        CVarDef.Create("tts.api_url", "", CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Auth token of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiToken =
        CVarDef.Create("tts.api_token", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Amount of seconds before timeout for API
    /// </summary>
    public static readonly CVarDef<int> TTSApiTimeout =
        CVarDef.Create("tts.api_timeout", 5, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Default volume setting of TTS sound for radio
    /// </summary>
    public static readonly CVarDef<float> TTSVolumeRadio =
        CVarDef.Create("tts.volume_radio", 0.5f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Default volume setting of TTS sound for others
    /// </summary>
    public static readonly CVarDef<float> TTSVolume =
        CVarDef.Create("tts.volume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Count of in-memory cached tts voice lines.
    /// </summary>
    public static readonly CVarDef<int> TTSMaxCache =
        CVarDef.Create("tts.max_cache", 250, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Enable a radio effect for TTS messages sent over radio channels.
    /// </summary>
    public static readonly CVarDef<bool> TTSRadioEffect =
        CVarDef.Create("scc.tts.radio_effect_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// The path to the FFmpeg executable for audio processing.
    /// </summary>
    public static readonly CVarDef<string> TTSFfmpegPath =
        CVarDef.Create("scc.tts.ffmpeg_path", "", CVar.SERVERONLY);

    public static readonly CVarDef<string> TTSFfmpegArguments =
        CVarDef.Create("scc.tts.ffmpeg_arguments", "-i pipe:0 -f ogg -v quiet -filter_complex \"[0:a]highpass=f=1000,lowpass=f=500[filtered];[filtered]acrusher=level_in=1:level_out=1:bits=4:mix=0.5:mode=log[crushed];[crushed]loudnorm=I=-12:LRA=7\" pipe:1",
            CVar.SERVERONLY);

    /*
     * Sponsors
     */

    /// <summary>
    ///     URL of the sponsors server API.
    /// </summary>
    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("sponsor.api_url", "", CVar.SERVERONLY);

    /*
     * Queue
     */

    /// <summary>
    ///     Controls if the connections queue is enabled. If enabled stop kicking new players after `SoftMaxPlayers` cap and instead add them to queue.
    /// </summary>
    public static readonly CVarDef<bool>
        QueueEnabled = CVarDef.Create("queue.enabled", false, CVar.SERVERONLY);

    /*
     * Discord Auth
     */

    /// <summary>
    ///     Enabled Discord linking, show linking button and modal window
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("discord_auth.enabled", false, CVar.SERVERONLY);

    /// <summary>
    ///     URL of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiUrl =
        CVarDef.Create("discord_auth.api_url", "", CVar.SERVERONLY);

    /// <summary>
    ///     Secret key of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiKey =
        CVarDef.Create("discord_auth.api_key", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     Getting up a character after falling
    /// </summary>
    public static readonly CVarDef<bool> AutoStanding =
        CVarDef.Create("control.auto_standing", false, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);
}
