# - playtime_reduceoverall
cmd-playtime_reduceoverall-desc = Убавляет указанное число минут из общего игрового времени игрока
cmd-playtime_reduceoverall-help = Использование: { $command } <user name> <minutes>
cmd-playtime_reduceoverall-succeed = Общее игровое время { $username } уменьшено на { TOSTRING($time, "dddd\\:hh\\:mm") }.
cmd-playtime_reduceoverall-arg-user = <user name>
cmd-playtime_reduceoverall-arg-minutes = <minutes>
cmd-playtime_reduceoverall-error-args = Ожидается ровно два аргумента

# - playtime_reducerole
cmd-playtime_reducerole-desc = Убавляет указанное число минут из времени игрока на определённой роли
cmd-playtime_reducerole-help = Использование: { $command } <user name> <role> <minutes>
cmd-playtime_reducerole-succeed = Игровое время для { $username } / \'{ $role }\' уменьшено на { TOSTRING($time, "dddd\\:hh\\:mm") }.
cmd-playtime_reducerole-arg-user = <user name>
cmd-playtime_reducerole-arg-role = <role>
cmd-playtime_reducerole-arg-minutes = <minutes>
cmd-playtime_reducerole-error-args = Ожидается ровно три аргумента
