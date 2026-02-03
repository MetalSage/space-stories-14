character-info-bank-briefing = [color=yellow]Вы помните данные своего банковского счета:[/color]
    Номер счёта: [color=white]{ $account }[/color]
    PIN-код: [color=white]{ $pin }[/color]

atm-ui-title = Банкомат
atm-ui-header-bank = Банк NanoTrasen
atm-ui-header-machine = Авт. кассовый терминал
atm-ui-welcome = Добро пожаловать
atm-ui-welcome-user = Привет, { $user }!
atm-ui-account-number = Номер счёта
atm-ui-pin = PIN-код
atm-ui-login-button = Войти
atm-ui-balance-label = БАЛАНС:
atm-ui-balance-value = { $balance } кр.
atm-ui-withdraw = Быстрое снятие
atm-ui-custom-amount = Другая сумма
atm-ui-withdraw-button = Снять
atm-ui-logout-button = Выйти
atm-ui-flavor-left = NanoTrasen Банк
atm-ui-flavor-right = v1.0
atm-ui-withdraw-option = { $amount } кр.

atm-msg-login-success = Успешный вход.
atm-msg-logged-out = Выход выполнен.
atm-msg-invalid-pin = Неверный PIN.
atm-msg-account-not-found = Счёт не найден.
atm-msg-insufficient-funds = Недостаточно средств.
atm-msg-withdraw-success = Снято { $amount } кр.
atm-msg-deposit-success = Внесено { $amount } кр.

vending-machine-ui-balance = Баланс: { $balance } кр.
vending-machine-ui-price = Цена
vending-machine-ui-amount = Количество
vending-machine-ui-dispense = Выдать
vending-machine-ui-select-item = Выберите товар
vending-machine-item-entry-with-price = { $name } [{ $amount }] - { $price } кр.
vending-machine-item-entry-no-price = { $name } [{ $amount }]
vending-machine-insufficient-funds = Недостаточно средств на счету!

bank-program-name = Банк NT
bank-ui-insert-id-label = Отключить уведомления
bank-ui-balance-label = Баланс:
bank-ui-account-number = Номер счета
bank-ui-transfer-header = Перевод средств
bank-ui-target-placeholder = Номер счета получателя
bank-ui-amount-placeholder = Сумма
bank-ui-transfer-btn = Перевод
bank-ui-settings-header = Настройки ID
bank-ui-link-btn = Привязать ID
bank-ui-unlink-btn = Отвязать ID
bank-ui-notifications-enable = Включить уведомления
bank-ui-notifications-disable = Отключить уведомления

bank-app-transfer-success = Перевод выполнен успешно.
bank-app-transfer-fail = Ошибка перевода. Проверьте баланс и номер счета.
bank-app-link-success = ID карта успешно привязана к счету.
bank-app-unlink-success = ID карта отвязана.

bank-app-notification-salary-title = Начисление заработной платы
bank-app-notification-salary-body = На ваш счет зачислено { $amount } кр. в качестве оплаты труда. Спасибо за службу NanoTrasen!
bank-app-notification-error-title = Системное оповещение
bank-app-notification-error-body = Внимание: Зафиксирован сбой транзакции. С вашего счета списано { $amount } кр. Обратитесь к представителю службы кадров.
bank-app-notification-fine-title = Уведомление о штрафе
bank-app-notification-fine-body = Согласно постановлению администрации, с вашего счета удержано { $amount } кр.
bank-app-notification-fine-percent-body = Удержан корпоративный налог { $percent }%: списано { $amount } кр.
bank-app-notification-admin-change-title = Корректировка счета
bank-app-notification-admin-change-body = Административное изменение баланса. Старый: { $old }, Новый: { $new }.
bank-app-notification-admin-add-body = Административное зачисление/списание средств: { $amount } кр.

stories-station-event-banking-error = Центральный банковский сервер NanoTrasen сообщает о повреждении базы данных сектора. Возможны спонтанные потери средств на личных счетах сотрудников.
stories-station-event-salary-increase = Центральное Командование высоко оценивает эффективность работы смены. Коэффициент заработной платы временно увеличен. Продолжайте в том же духе!
stories-station-event-salary-decrease = В связи с квартальной оптимизацией бюджета, коэффициенты заработной платы временно снижены. Приносим извинения за неудобства.
stories-station-event-bank-hack = Обнаружено несанкционированное вторжение в локальную банковскую подсеть. Активирован протокол "Обнуление" для предотвращения кражи данных. Все счета были обнулены.

cmd-econ-error-no-mind = У выбранного игрока отсутствует сознание (Mind).
cmd-econ-error-no-account = У игрока нет компонента банковского счета.
cmd-econ-error-station-account-missing = Не удалось найти запись о счете в базе данных станции.
cmd-econ-arg-player = <Имя игрока / UserID>
cmd-econ-arg-amount = <Сумма>
cmd-econ-arg-amount-delta = <Сумма (положительная или отрицательная)>
cmd-econ-arg-multiplier = [Множитель (стандарт: 1.0)]

cmd-econ-getbalance-success = Баланс игрока { $player }: { $balance } кр.
cmd-econ-setbalance-success = Баланс игрока { $player } успешно установлен на { $amount } кр.
cmd-econ-addbalance-success = Баланс игрока { $player } изменен на { $amount } кр.
cmd-econ-paysalary-success = Инициирована выплата зарплат всем сотрудникам с множителем { $multiplier }.
cmd-econ-fineall-success = Операция завершена. Оштрафовано { $count } счетов на сумму { $amount } кр.
