ore-silo-ui-title = Бункер Материалов
ore-silo-ui-label-clients = Machines
ore-silo-ui-label-mats = Машины
ore-silo-ui-itemlist-entry = {$linked ->
    [true] {"[Соединено] "}
    *[False] {""}
} {$name} ({$beacon}) {$inRange ->
    [true] {""}
    *[false] (Вне Диапазона)
}
