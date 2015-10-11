let cmdsx =
    [|
        "MINECART"
        """tellraw @a ["1  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["2  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["3  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["4  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["5  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["6  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["7  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["8  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["9  ",{"score":{"name":"Ticks","objective":"S"}}]"""
    |]
let cmdsxx =
    [|
        """R"""
        """O tellraw @a ["1  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["2  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["3  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["4  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["5  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["6  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["7  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["8  ",{"score":{"name":"Ticks","objective":"S"}}]"""
        """tellraw @a ["9  ",{"score":{"name":"Ticks","objective":"S"}}]"""
    |]

let cmds = // TODO make so player places 2 spawn eggs, it computes dx dy, draws line
    [|
        // assuming DX and DY are set in lineY objective, with DX >= 0, then this draws the line from player to relative (DX,DY) once player's lineY becomes non-zero
        yield "R"
        yield "P"
        // detect a scoreboard update and launch the mechanism
        yield "execute @p[score_lineY_min=1] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"lineYstart\"],Marker:1,NoGravity:1}"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players set @p lineY 0"
        // deal with negative DY
        yield "scoreboard players set @e[tag=lineYstart] lineYreversed 0"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players test DY lineY * -1"
        yield "C execute @e[tag=lineYstart] ~ ~ ~ scoreboard players set Temp lineY 0"
        yield "C execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation Temp lineY -= DY lineY"
        yield "C execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation DY lineY = Temp lineY"
        yield "C scoreboard players set @e[tag=lineYstart] lineYreversed 1"
        // init vars
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation Temp lineY = DX lineY"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation Temp lineY -= DY lineY"
        yield "scoreboard players set @e[tag=lineYstart] lineY 0"  // 0 means DY is greater
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players test Temp lineY 0 *"
        yield "C scoreboard players set @e[tag=lineYstart] lineY 1"  // 1 means DX is greater or equal
        yield "execute @e[tag=lineYstart,score_lineY=0] ~ ~ ~ scoreboard players operation DX lineY >< DY lineY"  // swap X/Y if DY was greater
        // setup '2dx/2dy' vars
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation TDY lineY = DY lineY"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation TDY lineY += TDY lineY"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation TDX lineY = DX lineY"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation TDX lineY += TDX lineY"
        // D = 2dy - dx
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation D lineY = TDY lineY"
        yield "execute @e[tag=lineYstart] ~ ~ ~ scoreboard players operation D lineY -= DX lineY"
        yield "entitydata @e[tag=lineYstart] {Tags:[\"lineYloop\"]}"
        // loop
        yield "execute @e[tag=lineYloop] ~ ~ ~ setblock ~ ~ ~ stone"
        yield "tp @e[tag=lineYloop,score_lineY_min=1] ~1 ~ ~"
        yield "tp @e[tag=lineYloop,score_lineY=0,score_lineYreversed=0] ~ ~ ~1"
        yield "tp @e[tag=lineYloop,score_lineY=0,score_lineYreversed_min=1] ~ ~ ~-1"
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players test D lineY 1 *"
        yield "C tp @e[tag=lineYloop,score_lineY_min=1,score_lineYreversed=0] ~ ~ ~1"
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players test D lineY 1 *"
        yield "C tp @e[tag=lineYloop,score_lineY_min=1,score_lineYreversed_min=1] ~ ~ ~-1"
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players test D lineY 1 *"
        yield "C tp @e[tag=lineYloop,score_lineY=0] ~1 ~ ~"
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players test D lineY 1 *"
        yield "C execute @e[tag=lineYloop] ~ ~ ~ scoreboard players operation D lineY -= TDX lineY"
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players operation D lineY += TDY lineY"
        // endloop, stop when done dx steps
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players remove DX lineY 1"
        yield "execute @e[tag=lineYloop] ~ ~ ~ scoreboard players test DX lineY * -1"
        yield "C kill @e[tag=lineYloop]"
        // init bit
        yield "R"
        yield "O gamerule commandBlockOutput false"
        yield "scoreboard objectives add lineY dummy"
        yield "scoreboard objectives add lineYreversed dummy"
        yield "scoreboard players set @a lineY 0"
        yield """tellraw @a {"text":"Initializing, wait one moment...","color":"red"}"""
    |]

let escape(s:string) = s.Replace("\"","\\\"")
let escape2(s) = escape(s).Replace("\"","^").Replace("\\","\\\\").Replace("^","\\\"")

let BLOCKS = true


[<System.STAThreadAttribute>]
do
    let mutable sb = new System.Text.StringBuilder()
    if cmds.[0] <> "MINECART" then
#if RIDING        
        sb.Append("""summon Item ~ ~10 ~ {Item:{id:dirt,Damage:0,Count:1},Age:5999,""") |> ignore
        for c in cmds do
            if c = "R" then
                sb.Append("Riding:{id:FallingSand,Block:redstone_block,Time:1,") |> ignore 
            elif c = "P" then
                sb.Append("Riding:{id:FallingSand,Block:repeating_command_block,Time:1,") |> ignore
            elif c.StartsWith("O ") then
                sb.Append(sprintf "Riding:{id:FallingSand,Block:command_block,Time:1,TileEntityData:{Command:\"%s\"}," (escape (c.Substring(2)))) |> ignore
            elif c.StartsWith("C ") then
                sb.Append(sprintf "Riding:{id:FallingSand,Block:chain_command_block,Time:1,Data:8,TileEntityData:{Command:\"%s\"}," (escape (c.Substring(2)))) |> ignore
            else
                sb.Append(sprintf "Riding:{id:FallingSand,Block:chain_command_block,Time:1,TileEntityData:{Command:\"%s\"}," (escape c)) |> ignore
        sb <- new System.Text.StringBuilder(sb.ToString().Substring(0,sb.Length-1))  // strip extra comma
        for i = 0 to cmds.Length do
            sb.Append("""}""") |> ignore
#else
        sb.Append("""summon Item ~ ~0.55 ~ {Item:{id:dirt,Damage:0,Count:1},Age:5999,Passengers:[""") |> ignore
        for c in cmds |> Seq.toList |> List.rev do
            if c = "R" then
                sb.Append("{id:FallingSand,Block:redstone_block,Time:1,Passengers:[") |> ignore 
            elif c = "P" then
                sb.Append("{id:FallingSand,Block:repeating_command_block,Time:1,Passengers:[") |> ignore
            elif c.StartsWith("O ") then
                sb.Append(sprintf "{id:FallingSand,Block:command_block,Time:1,TileEntityData:{Command:\"%s\"},Passengers:[" (escape (c.Substring(2)))) |> ignore
            elif c.StartsWith("C ") then
                sb.Append(sprintf "{id:FallingSand,Block:chain_command_block,Time:1,Data:8,TileEntityData:{Command:\"%s\"},Passengers:[" (escape (c.Substring(2)))) |> ignore
            else
                sb.Append(sprintf "{id:FallingSand,Block:chain_command_block,Time:1,TileEntityData:{Command:\"%s\"},Passengers:[" (escape c)) |> ignore
        for i = 0 to cmds.Length do
            sb.Append("""]}""") |> ignore
#endif
    else
#if RIDING        
        // runs all cmds in 1 tick, after 8 ticks, in reverse order, leaves only tiny residue
        sb.Append("""summon Item ~ ~1 ~ {Item:{id:dirt,Damage:0,Count:1},Age:5999,""") |> ignore
        for c in cmds do
            if c = "MINECART" then // first cmd
                sb.Append("Riding:{id:MinecartCommandBlock,Command:\"kill @e[type=MinecartCommandBlock]\",") |> ignore 
//                sb.Append("Riding:{id:MinecartCommandBlock,Command:\"fill ~ ~-2 ~ ~ ~ ~ air\",") |> ignore 
            else
                sb.Append(sprintf "Riding:{id:MinecartCommandBlock,Command:\"%s\"," (escape c)) |> ignore
        sb.Append("Riding:{id:MinecartCommandBlock,Command:\"say I am a dummy\",") |> ignore
        sb.Append("Riding:{id:FallingSand,Block:activator_rail,Time:1,") |> ignore 
        sb.Append("Riding:{id:FallingSand,Block:redstone_block,Time:1,") |> ignore 
        sb <- new System.Text.StringBuilder(sb.ToString().Substring(0,sb.Length-1))  // strip extra comma
        for i = 0 to cmds.Length+3 do
            sb.Append("""}""") |> ignore
#else
        // runs all cmds in 1 tick, 8 ticks after activation, leaves only original block
        sb.Append(sprintf """summon FallingSand ~ ~0.55 ~ {Block:command_block,Time:1,TileEntityData:{ Command:"summon MinecartCommandBlock ~ ~1.5 ~ {Command:\"%s\",Passengers:[""" (escape2 cmds.[1])) |> ignore
        for i = 2 to cmds.Length-1 do
            sb.Append(sprintf """{id:MinecartCommandBlock,Command:\"%s\"},""" (escape2 cmds.[i])) |> ignore
        sb.Append("""{id:MinecartCommandBlock,Command:\"setblock ~ ~-2 ~ command_block 0 0 {auto:1b,Command:\\\"fill ~ ~ ~ ~ ~2 ~ air\\\"}\"},{id:MinecartCommandBlock,Command:\"kill @e[type=MinecartCommandBlock,r=5]\"}]}"},Passengers:[{id:FallingSand,Block:redstone_block,Time:1,Passengers:[{id:FallingSand,Block:activator_rail,Time:1}]}]}""") |> ignore
#endif

    System.Windows.Clipboard.SetText(sb.ToString())
    printfn "%s" (sb.ToString())
    printfn ""
    printfn "left  %d" (sb.ToString() |> Seq.filter (fun c -> c='{') |> Seq.length)
    printfn "right %d" (sb.ToString() |> Seq.filter (fun c -> c='}') |> Seq.length)
    printfn "chars %d" (sb.ToString().Length)
    printfn "blocks %d" (cmds.Length)
    printfn ""
    printfn "it's now in your clipboard"



