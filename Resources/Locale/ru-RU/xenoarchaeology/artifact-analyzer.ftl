analysis-console-menu-title = Аналитическая-Консоль Mark 3 Широкого Спектра Действия
analysis-console-server-list-button = Сервера
analysis-console-extract-button = Извлечь О.И.

analysis-console-info-no-scanner = Анализатор не подключен! Пожалуйста, подключите его с помощью мультитула.
analysis-console-info-no-artifact = Артефакт отсутствует! Поместите его на панель, чтобы увидеть информацию об узлах.
analysis-console-info-ready = Системы работают. Все готово к сканированию.

analysis-console-no-node = Выберите узел для просмотра
analysis-console-info-id = [font="Monospace" size=11]ID:[/font]
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{$id}[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Класс:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Статус:[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Заблокировано
    [1] lime]Разблокировано
    *[2] plum]Активно
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Долговечность:[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={$color}]{$current}/{$max}[/color][/font]
analysis-console-info-effect = [font="Monospace" size=11]Реакция:[/font]
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [true] {$info}
    *[false] Разблокируйте узлы, чтобы получить информацию
}[/color][/font]
analysis-console-info-trigger = [font="Monospace" size=11]Стимуляторы:[/font]
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{$triggers}[/color][/font]
analysis-console-info-scanner = Сканирование...
analysis-console-info-scanner-paused = Приостановлено.
analysis-console-progress-text = {$seconds ->
    [one] T-{$seconds} секунда
    *[other] T-{$seconds} секунд
}

analysis-console-extract-value = [font="Monospace" size=11][color=orange]Узел {$id} (+{$value})[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] Ни у одного из разблокированных узлов не осталось точек для извлечения [/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Общие Исследования: {$value}[/color][/font]

analyzer-artifact-extract-popup = Поверхность артефакта мерцает энергией!
