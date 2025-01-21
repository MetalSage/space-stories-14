discord-watchlist-connection-header =
    { $players ->
        [one] {$players} у игрока, включенного в список наблюдения, есть
        *[other] {$players} игроки, включенные в список наблюдения, имеют
    } подключение к {$serverName}

discord-watchlist-connection-entry = - {$playerName} с сообщением "{$message}"{ $expiry ->
        [0] {""}
        *[other] {" "}(expires <t:{$expiry}:R>)
    }{ $otherWatchlists ->
        [0] {""}
        [one] {" "}и {$otherWatchlists} другой список наблюдения
        *[other] {" "}и {$otherWatchlists} другой список наблюдения
    }
