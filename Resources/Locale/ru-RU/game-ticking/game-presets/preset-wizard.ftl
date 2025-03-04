## Survivor

roles-antag-survivor-name = Выживший
# It's a Halo reference
roles-antag-survivor-objective = Текущая цель: Выжить

survivor-role-greeting =
    Ты - Выживший.
    Прежде всего, вам нужно вернуться в ЦентКом живым.
    Соберите столько огневой мощи, сколько необходимо, чтобы гарантировать себе выживание.
    Не доверяй никому.

survivor-round-end-dead-count =
{
    $deadCount ->
        [one] [color=red]{$deadCount}[/color] выживший умер.
        *[other] [color=red]{$deadCount}[/color] выжившие умерли.
}

survivor-round-end-alive-count =
{
    $aliveCount ->
        [one] [color=yellow]{$aliveCount}[/color] оставшийся в живых был брошен на станции.
        *[other] [color=yellow]{$aliveCount}[/color] выжившие были брошены на станции.
}

survivor-round-end-alive-on-shuttle-count =
{
    $aliveCount ->
        [one] [color=green]{$aliveCount}[/color] выживший выбрался оттуда живым.
        *[other] [color=green]{$aliveCount}[/color] выжившие выбрались оттуда живыми.
}

## Wizard

objective-issuer-swf = [color=turquoise]Космическая Федерация Волшебников[/color]

wizard-title = Волшебник
wizard-description = На станции есть Волшебник! Никогда не знаешь, что они могут натворить.

roles-antag-wizard-name = Волшебник
roles-antag-wizard-objective = Преподай им урок, который они никогда не забудут.

wizard-role-greeting =
    ТЫ ВОЛШЕБНИК!
    Между Космической Федерации Волшебников и Нанотразеней возникли тёрки.
    Итак, Федерация космических волшебников выбрала вас для посещения станции.
    Продемонстрируйте им свои способности.
    Что вы будете делать, зависит только от вас, просто помните, что Космическая Федерация Волшебников хочет, чтобы вы выбрались оттуда живыми.

wizard-round-end-name = волшебник

## TODO: Wizard Apprentice (Coming sometime post-wizard release)
