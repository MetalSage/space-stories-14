vending-machine-restock-invalid-inventory = { CAPITALIZE($this) } не подходит для того, чтобы пополнить { $target }.
vending-machine-restock-needs-panel-open = Техническая панель { CAPITALIZE($target) } должна быть открыта.
vending-machine-restock-start-self = Вы начинаете пополнять { THE($target) }.
vending-machine-restock-start-others =
     { $user } { GENDER($user) ->
         [male] начал
         [female] начала
         [epicene] начали
        *[neuter] начало
     } пополнять { $target }.
vending-machine-restock-done-self = Вы закончили пополнять { $target }.
vending-machine-restock-done-others =
     { $user } { GENDER($user) ->
         [male] закончил
         [female] закончила
         [epicene] закончили
        *[neuter] закончило
     } пополнять { $target }.
