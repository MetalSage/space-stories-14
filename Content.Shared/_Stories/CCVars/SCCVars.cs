using Robust.Shared.Configuration;

namespace Content.Shared._Stories.SCCVars;

/// <summary>
/// Консольные переменные модулей Stories.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class SCCVars
{
    /*
     * TTS (Синтез речи)
     */

    /// <summary>
    /// Включена ли система TTS на сервере.
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabled =
        CVarDef.Create("stories.tts.enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Включена ли система TTS на клиенте.
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabledClient =
        CVarDef.Create("stories.tts.enabled_client", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// URL-адрес API сервера TTS.
    /// </summary>
    public static readonly CVarDef<string> TTSApiUrl =
        CVarDef.Create("stories.tts.api_url", "", CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Токен авторизации API сервера TTS.
    /// </summary>
    public static readonly CVarDef<string> TTSApiToken =
        CVarDef.Create("stories.tts.api_token", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Таймаут запросов к API TTS в секундах.
    /// </summary>
    public static readonly CVarDef<int> TTSApiTimeout =
        CVarDef.Create("stories.tts.api_timeout", 5, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Общая громкость звука TTS (мастер-громкость).
    /// </summary>
    public static readonly CVarDef<float> TTSVolumeMaster =
        CVarDef.Create("stories.tts.volume_master", 1.0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Стандартная громкость звука TTS для персонажей поблизости (PVS).
    /// </summary>
    public static readonly CVarDef<float> TTSVolumeNearby =
        CVarDef.Create("stories.tts.volume_nearby", 1.0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Стандартная громкость звука TTS для радио.
    /// </summary>
    public static readonly CVarDef<float> TTSVolumeRadio =
        CVarDef.Create("stories.tts.volume_radio", 0.5f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Стандартная громкость звука TTS для остальных.
    /// </summary>
    public static readonly CVarDef<float> TTSVolume =
        CVarDef.Create("stories.tts.volume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Громкость отдельных радиоканалов TTS.
    /// </summary>
    public static readonly CVarDef<string> TTSRadioVolumes =
        CVarDef.Create("stories.tts.radio_volumes", "{}", CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Стандартная громкость звука TTS для объявлений.
    /// </summary>
    public static readonly CVarDef<float> TTSVolumeAnnounce =
        CVarDef.Create("stories.tts.volume_announce", 1.0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Прототип голоса по умолчанию для станционных объявлений.
    /// </summary>
    public static readonly CVarDef<string> TTSAnnounceVoice =
        CVarDef.Create("stories.tts.announce_voice", "glados", CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Количество кэшируемых в памяти голосовых реплик TTS.
    /// </summary>
    public static readonly CVarDef<int> TTSMaxCache =
        CVarDef.Create("stories.tts.max_cache", 250, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Включить радиоэффект для сообщений TTS, передаваемых по каналам радио.
    /// </summary>
    public static readonly CVarDef<bool> TTSRadioEffect =
        CVarDef.Create("stories.tts.radio_effect_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// Путь к исполняемому файлу FFmpeg для обработки звука.
    /// </summary>
    public static readonly CVarDef<string> TTSFfmpegPath =
        CVarDef.Create("stories.tts.ffmpeg_path", "", CVar.SERVERONLY);

    /// <summary>
    /// Аргументы командной строки FFmpeg для обработки звука TTS.
    /// </summary>
    public static readonly CVarDef<string> TTSFfmpegArguments =
        CVarDef.Create("stories.tts.ffmpeg_arguments",
            "-i pipe:0 -f ogg -v quiet -filter_complex \"[0:a]highpass=f=1000,lowpass=f=500[filtered];[filtered]acrusher=level_in=1:level_out=1:bits=4:mix=0.5:mode=log[crushed];[crushed]loudnorm=I=-12:LRA=7\" pipe:1",
            CVar.SERVERONLY);

    /// <summary>
    /// FFmpeg аудиофильтр для стандартного радио TTS.
    /// </summary>
    public static readonly CVarDef<string> TTSStandardRadioFfmpegFilter =
        CVarDef.Create("stories.tts.standard_radio_ffmpeg_filter",
            "highpass=f=400,lowpass=f=2500,volume=2.0",
            CVar.SERVERONLY);

    /// <summary>
    /// Включить эффект объявления для станционных оповещений TTS.
    /// </summary>
    public static readonly CVarDef<bool> TTSAnnounceEffect =
        CVarDef.Create("stories.tts.announce_effect_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// FFmpeg аудиофильтр для станционных объявлений TTS.
    /// </summary>
    public static readonly CVarDef<string> TTSAnnounceFfmpegFilter =
        CVarDef.Create("stories.tts.announce_ffmpeg_filter",
            "aecho=0.8:0.88:60:0.4,equalizer=f=1000:width_type=h:width=200:g=3,volume=1.5",
            CVar.SERVERONLY);

    /*
     * Спонсоры
     */

    /// <summary>
    /// URL-адрес API сервера спонсоров.
    /// </summary>
    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("stories.sponsor.api_url", "", CVar.SERVERONLY);

    /*
     * Очередь подключения
     */

    /// <summary>
    /// Управляет включением очереди подключения. Если включено, перестает кикать новых игроков
    /// после превышения лимита `SoftMaxPlayers` и добавляет их в очередь.
    /// </summary>
    public static readonly CVarDef<bool> QueueEnabled =
        CVarDef.Create("stories.queue.enabled", false, CVar.SERVERONLY);

    /*
     * Авторизация через Discord
     */

    /// <summary>
    /// Включить привязку Discord, отображение кнопки привязки и модального окна.
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("stories.discord_auth.enabled", false, CVar.SERVERONLY);

    /// <summary>
    /// URL-адрес API сервера авторизации Discord.
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiUrl =
        CVarDef.Create("stories.discord_auth.api_url", "", CVar.SERVERONLY);

    /// <summary>
    /// Секретный ключ API сервера авторизации Discord.
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiKey =
        CVarDef.Create("stories.discord_auth.api_key", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /*
     * Управление
     */

    /// <summary>
    /// Автоматический подъем персонажа на ноги после падения.
    /// </summary>
    public static readonly CVarDef<bool> AutoStanding =
        CVarDef.Create("stories.control.auto_standing", false, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    /*
     * Экономика
     */

    /// <summary>
    /// Частота начисления зарплаты (в минутах).
    /// </summary>
    public static readonly CVarDef<float> EconomySalaryFrequency =
        CVarDef.Create("stories.economy.salary_frequency", 15f, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Процент начисляемой зарплаты.
    /// </summary>
    public static readonly CVarDef<float> EconomySalaryPercentage =
        CVarDef.Create("stories.economy.salary_percentage", 0.5f, CVar.SERVERONLY | CVar.ARCHIVE);

    /*
     * Предупреждение о EORG в конце раунда
     */

    /// <summary>
    /// Включено ли всплывающее окно с предупреждением о запрете EORG в конце раунда.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndNoEorgPopup =
        CVarDef.Create("stories.round_end.eorg_popup_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Пропускать ли всплывающее окно о EORG в конце раунда (настройка клиента).
    /// </summary>
    public static readonly CVarDef<bool> SkipRoundEndNoEorgPopup =
        CVarDef.Create("stories.round_end.eorg_popup_skip", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Время отображения всплывающего окна о EORG в конце раунда (в секундах).
    /// </summary>
    public static readonly CVarDef<float> RoundEndNoEorgPopupTime =
        CVarDef.Create("stories.round_end.eorg_popup_time", 5f, CVar.SERVER | CVar.REPLICATED);

    /*
     * Тенеморф
     */

    /// <summary>
    /// Игнорировать требование наличия разума для подчинения тенеморфом.
    /// </summary>
    public static readonly CVarDef<bool> EnthrallWithoutMind =
        CVarDef.Create("stories.shadowling.enthrall_without_mind", false, CVar.SERVERONLY);

    /*
     * Космическая тюрьма (КТ)
     */

    /// <summary>
    /// Включена ли станция Космической тюрьмы и разрешено ли ее появление.
    /// </summary>
    public static readonly CVarDef<bool> PrisonEnabled =
        CVarDef.Create("stories.prison.enabled", false, CVar.SERVERONLY | CVar.ARCHIVE);

    /*
     * Спонсоры
     */

    /// <summary>
    /// Выбранный спонсорский скин призрака.
    /// </summary>
    public static readonly CVarDef<string> SelectedGhostSkin =
        CVarDef.Create("stories.sponsor.ghost_skin", "", CVar.CLIENTONLY | CVar.ARCHIVE);

    /*
     * Голосование
     */

    /// <summary>
    /// Включает сохранение голосов для невыбранных карт между голосованиями.
    /// </summary>
    public static readonly CVarDef<bool> VoteMapCarryover =
        CVarDef.Create("stories.vote_map_carryover", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Включает сохранение голосов для невыбранных режимов игры между голосованиями.
    /// </summary>
    public static readonly CVarDef<bool> VotePresetCarryover =
        CVarDef.Create("stories.vote_preset_carryover", true, CVar.SERVERONLY | CVar.ARCHIVE);
}
