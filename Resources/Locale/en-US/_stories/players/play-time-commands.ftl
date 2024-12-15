# - playtime_reduceoverall
cmd-playtime_reduceoverall-desc = reduces the specified minutes to a player's overall playtime
cmd-playtime_reduceoverall-help = Usage: {$command} <user name> <minutes>
cmd-playtime_reduceoverall-succeed = Increased overall time for {$username} to {TOSTRING($time, "dddd\\:hh\\:mm")}
cmd-playtime_reduceoverall-arg-user = <user name>
cmd-playtime_reduceoverall-arg-minutes = <minutes>
cmd-playtime_reduceoverall-error-args = Expected exactly two arguments

# - playtime_reducerole
cmd-playtime_reducerole-desc = reduces the specified minutes to a player's role playtime
cmd-playtime_reducerole-help = Usage: {$command} <user name> <role> <minutes>
cmd-playtime_reducerole-succeed = Increased role playtime for {$username} / \'{$role}\' to {TOSTRING($time, "dddd\\:hh\\:mm")}
cmd-playtime_reducerole-arg-user = <user name>
cmd-playtime_reducerole-arg-role = <role>
cmd-playtime_reducerole-arg-minutes = <minutes>
cmd-playtime_reducerole-error-args = Expected exactly three arguments
