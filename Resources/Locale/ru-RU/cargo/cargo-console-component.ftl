## UI
cargo-console-menu-title = Консоль заказа грузов
cargo-console-menu-account-name-label = Аккаунт:{ " " }
cargo-console-menu-account-name-none-text = Нет
cargo-console-menu-account-name-format = [bold][color={$color}]{$name}[/color][/bold] [font="Monospace"]\[{$code}\][/font]
cargo-console-menu-shuttle-name-label = Название шаттла:{ " " }
cargo-console-menu-shuttle-name-none-text = Нет
cargo-console-menu-points-label = Баланс:{ " " }
cargo-console-menu-points-amount = ${ $amount }
cargo-console-menu-shuttle-status-label = Статус шаттла:{ " " }
cargo-console-menu-shuttle-status-away-text = Отбыл
cargo-console-menu-order-capacity-label = Объём заказов:{ " " }
cargo-console-menu-call-shuttle-button = Активировать телепад
cargo-console-menu-permissions-button = Доступы
cargo-console-menu-categories-label = Категории:{ " " }
cargo-console-menu-search-bar-placeholder = Поиск
cargo-console-menu-requests-label = Запросы
cargo-console-menu-orders-label = Заказы
cargo-console-menu-order-reason-description = Причина: { $reason }
cargo-console-menu-populate-categories-all-text = Все
cargo-console-menu-populate-orders-cargo-order-row-product-name-text = {$productName} (x{$orderAmount}) от {$orderRequester} из [color={$accountColor}]{$account}[/color]
cargo-console-menu-cargo-order-row-approve-button = Одобрить
cargo-console-menu-cargo-order-row-cancel-button = Отменить
cargo-console-menu-tab-title-orders = Заказы
cargo-console-menu-tab-title-funds = Переводы
cargo-console-menu-account-action-transfer-limit = [bold]Лимит перевода:[/bold] ${$limit}
cargo-console-menu-account-action-transfer-limit-unlimited-notifier = [color=gold](Безлимитный)[/color]
cargo-console-menu-account-action-select = [bold]Действие с Аккаунтом:[/bold]
cargo-console-menu-account-action-amount = [bold]Сумма:[/bold] $
cargo-console-menu-account-action-button = Перевести
cargo-console-menu-toggle-account-lock-button = Переключить Лимит Перевода
cargo-console-menu-account-action-option-withdraw = Снять Наличные
cargo-console-menu-account-action-option-transfer = Перевести Средства на {$code}

# Orders
cargo-console-order-not-allowed = Доступ запрещён
cargo-console-station-not-found = Нет доступной станции
cargo-console-invalid-product = Неверный ID продукта
cargo-console-too-many = Слишком много одобренных заказов
cargo-console-snip-snip = Заказ урезан до вместимости
cargo-console-insufficient-funds = Недостаточно средств (требуется { $cost })
cargo-console-unfulfilled = Нет места для выполнения заказа
cargo-console-trade-station = Отправлено на { $destination }
cargo-console-unlock-approved-order-broadcast = [bold]Заказ на { $productName } x{ $orderAmount }[/bold], стоимостью [bold]{ $cost }[/bold], был одобрен [bold]{ $approver }[/bold]
cargo-console-paper-reason-default = Нет
cargo-console-paper-approver-default = Сам
cargo-console-paper-print-name = Заказ #{ $orderNumber }
cargo-console-paper-print-text = [head=2]Заказ #{$orderNumber}[/head]
    {"[bold]Товар:[/bold]"} {$itemName} (x{$orderQuantity})
    {"[bold]Запросил:[/bold]"} {$requester}

    {"[head=3]Информация Заказа[/head]"}
    {"[bold]Плательщик[/bold]:"} {$account} [font="Monospace"]\[{$accountcode}\][/font]
    {"[bold]Одобрено:[/bold]"} {$approver}
    {"[bold]Причина:[/bold]"} {$reason}
cargo-console-fund-withdraw-broadcast = [bold]{$name} снял {$amount} кредитов с {$name1} \[{$code1}\]
cargo-console-fund-transfer-broadcast = [bold]{$name} перевёл {$amount} кредиты с {$name1} \[{$code1}\] на {$name2} \[{$code2}\][/bold]
cargo-console-fund-transfer-user-unknown = Неизвестно

# Cargo shuttle console
cargo-shuttle-console-menu-title = Консоль вызова грузового шаттла
cargo-shuttle-console-station-unknown = Неизвестно
cargo-shuttle-console-shuttle-not-found = Не найден
cargo-shuttle-console-organics = На шаттле обнаружены органические формы жизни
cargo-no-shuttle = Грузовой шаттл не найден!

# Funding allocation console
cargo-funding-alloc-console-menu-title = Консоль Распределения Финансирования
cargo-funding-alloc-console-label-account = [bold]Аккаунт[/bold]
cargo-funding-alloc-console-label-code = [bold] Код [/bold]
cargo-funding-alloc-console-label-balance = [bold] Баланс [/bold]
cargo-funding-alloc-console-label-cut = [bold] Доля Дохода (%) [/bold]

cargo-funding-alloc-console-label-primary-cut = Доля Карго от средств из источников, не связанных с заблокированным ящиком (%):
cargo-funding-alloc-console-label-lockbox-cut = Доля Карго от средств от продаж из заблокированного ящика (%):

cargo-funding-alloc-console-label-help-non-adjustible = Карго получает {$percent}% прибыли от продаж, не связанных с заблокированным ящиком. Остальная часть распределяется, как указано ниже:
cargo-funding-alloc-console-label-help-adjustible = Оставшиеся средства из источников, не связанных с заблокированным ящиком, распределяются, как указано ниже:
cargo-funding-alloc-console-button-save = Сохранить Изменения
cargo-funding-alloc-console-label-save-fail = [bold]Revenue Divisions Invalid![/bold] [color=red]({$pos ->
    [1] +
    *[-1] -
}{$val}%)[/color]

# Slip template
cargo-acquisition-slip-body = [head=3]Информация Товара[/head]
    {"[bold]Товар:[/bold]"} {$product}
    {"[bold]Описание:[/bold]"} {$description}
    {"[bold]Стоимость за единицу:[/bold"}] ${$unit}
    {"[bold]Количество:[/bold]"} {$amount}
    {"[bold]Стоимость:[/bold]"} ${$cost}

    {"[head=3]Информация Покупки[/head]"}
    {"[bold]Заказчик:[/bold]"} {$orderer}
    {"[bold]Причина:[/bold]"} {$reason}
