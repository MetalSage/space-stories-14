ore-silo-ui-title = Бункер Ресурсов
ore-silo-ui-label-clients = Оборудование
ore-silo-ui-label-mats = Ресурсы
ore-silo-ui-itemlist-entry = {$linked ->
    [true] {"[Соединено] "}
    *[False] {""}
} {$name} ({$beacon}) {$inRange ->
    [true] {""}
    *[false] (Вне Диапазона)
}
