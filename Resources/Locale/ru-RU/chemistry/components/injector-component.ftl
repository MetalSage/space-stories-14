## UI

injector-volume-transfer-label = Объём: [color=white]{$currentVolume}/{$totalVolume} ед.[/color]
    Режим: [color=white]{$modeString}[/color] ([color=white]{$transferVolume} ед.[/color])
injector-volume-label = Объём: [color=white]{$currentVolume}/{$totalVolume} ед.[/color]
    Режим: [color=white]{$modeString}[/color]
injector-toggle-verb-text = Переключить режим инъектора

## Entity

injector-component-inject-mode-name = ввод
injector-component-draw-mode-name = забор
injector-component-dynamic-mode-name = динамический
injector-component-mode-changed-text = Установлен {$mode}
injector-component-transfer-success-message = Вы перемещаете { $amount } ед. в { $target }.
injector-component-transfer-success-message-self = Вы перемещаете в себя { $amount } ед.
injector-component-inject-success-message = Вы вводите { $amount } ед. в { $target }!
injector-component-inject-success-message-self = Вы вводите в себя { $amount } ед.!
injector-component-draw-success-message = Вы набираете { $amount } ед. из { $target }.
injector-component-draw-success-message-self = Вы набираете из себя { $amount } ед.

## Fail Messages

injector-component-target-already-full-message = { CAPITALIZE($target) } полон!
injector-component-target-already-full-message-self = Вы уже полны!
injector-component-target-is-empty-message = { CAPITALIZE($target) } пуст!
injector-component-target-is-empty-message-self = Вы пусты!
injector-component-cannot-toggle-draw-message = Больше не набрать!
injector-component-cannot-toggle-inject-message = Нечего вводить!
injector-component-cannot-toggle-dynamic-message = Нельзя включить динамический!
injector-component-empty-message = {CAPITALIZE($injector)} пуст!
injector-component-blocked-user = Защитное снаряжение блокирует инъекцию!
injector-component-blocked-other = {CAPITALIZE(POSS-ADJ($target))} броня защитила от инъекции {$user}!
injector-component-cannot-transfer-message = Вы не можете ничего переместить в { $target }!
injector-component-cannot-transfer-message-self = Вы не можете ничего переместить в себя!
injector-component-cannot-inject-message = Вы не можете ничего ввести в { $target }!
injector-component-cannot-inject-message-self = Вы не можете ничего себе ввести!
injector-component-cannot-draw-message = Вы не можете ничего набрать из { $target }!
injector-component-cannot-draw-message-self = Вы не можете ничего набрать из себя!
injector-component-ignore-mobs = Этот инъектор может взаимодействовать только с контейнерами!

## mob-inject doafter messages

injector-component-needle-injecting-user = Вы начинаете вводить содержимое шприца.
injector-component-needle-injecting-target = {CAPITALIZE($user)} пытается ввести в вас шприц!
injector-component-needle-drawing-user = Вы начинаете набирать шприц.
injector-component-needle-drawing-target = {CAPITALIZE($user)} пытается набрать шприц из вас!
injector-component-spray-injecting-user = Вы подготавливаете впрыскивающее сопло.
injector-component-spray-injecting-target = {CAPITALIZE($user)} пытается приложить к вам впрыскивающее сопло!

## Target Popup Success messages
injector-component-feel-prick-message = Вы чувствуете слабый укольчик!
