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

let cmdsline = // TODO make so player places 2 spawn eggs, it computes dx dy dz, draws line
    [|
        // assuming DX DY and DZ are set in lineDraw objective, then this draws the line from player to relative (DX,DY,DZ) once player's lineDraw becomes non-zero
        yield "R"
        yield "P"
        // detect a scoreboard update and launch the mechanism
        yield "execute @p[score_lineDraw_min=1] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"lineDrawstart\"],Marker:1,NoGravity:1}"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players set @p lineDraw 0"
        // deal with negative DX
        yield "scoreboard players set @e[tag=lineDrawstart] lineDrawXrev 0"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players test DX lineDraw * -1"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players set Temp lineDraw 0"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation Temp lineDraw -= DX lineDraw"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation DX lineDraw = Temp lineDraw"
        yield "C scoreboard players set @e[tag=lineDrawstart] lineDrawXrev 1"
        // deal with negative DY
        yield "scoreboard players set @e[tag=lineDrawstart] lineDrawYrev 0"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players test DY lineDraw * -1"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players set Temp lineDraw 0"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation Temp lineDraw -= DY lineDraw"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation DY lineDraw = Temp lineDraw"
        yield "C scoreboard players set @e[tag=lineDrawstart] lineDrawYrev 1"
        // deal with negative DZ
        yield "scoreboard players set @e[tag=lineDrawstart] lineDrawZrev 0"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players test DZ lineDraw * -1"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players set Temp lineDraw 0"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation Temp lineDraw -= DZ lineDraw"
        yield "C execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation DZ lineDraw = Temp lineDraw"
        yield "C scoreboard players set @e[tag=lineDrawstart] lineDrawZrev 1"
        // init vars, choose major axis
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TempZ lineDraw = DX lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TempZ lineDraw -= DZ lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players set @e[tag=lineDrawstart] lineDraw 0"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation MAJOR lineDraw = DZ lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players test TempZ lineDraw 0 *"
        yield "C scoreboard players set @e[tag=lineDrawstart] lineDraw 1"
        yield "C scoreboard players operation MAJOR lineDraw = DX lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TempY lineDraw = DY lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TempY lineDraw -= MAJOR lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players test TempY lineDraw 0 *"
        yield "C scoreboard players set @e[tag=lineDrawstart] lineDraw 2"
        yield "C scoreboard players operation MAJOR lineDraw = DY lineDraw"
        // setup '2dx/2dy/2dz' vars
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TDZ lineDraw = DZ lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TDZ lineDraw += TDZ lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TDX lineDraw = DX lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TDX lineDraw += TDX lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TDY lineDraw = DY lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TDY lineDraw += TDY lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TMAJOR lineDraw = MAJOR lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation TMAJOR lineDraw += TMAJOR lineDraw"
        // A = 2dz - dx
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation AZ lineDraw = TDZ lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation AZ lineDraw -= MAJOR lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation AY lineDraw = TDY lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation AY lineDraw -= MAJOR lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation AX lineDraw = TDX lineDraw"
        yield "execute @e[tag=lineDrawstart] ~ ~ ~ scoreboard players operation AX lineDraw -= MAJOR lineDraw"
        yield "entitydata @e[tag=lineDrawstart] {Tags:[\"lineDrawloop\"]}"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ setblock ~ ~ ~ stone"
        // loop
(*
        // - major axis
        yield "tp @e[tag=lineDrawloop,score_lineDraw=0,score_lineDraw_min=0,score_lineDrawZrev=0] ~ ~ ~1"
        yield "tp @e[tag=lineDrawloop,score_lineDraw=0,score_lineDraw_min=0,score_lineDrawZrev_min=1] ~ ~ ~-1"
        yield "tp @e[tag=lineDrawloop,score_lineDraw=2,score_lineDraw_min=0,score_lineDrawYrev=0] ~ ~1 ~"
        yield "tp @e[tag=lineDrawloop,score_lineDraw=2,score_lineDraw_min=0,score_lineDrawYrev_min=1] ~ ~-1 ~"
        yield "tp @e[tag=lineDrawloop,score_lineDraw=1] ~1 ~ ~"
*)
        // - diff on X
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AX lineDraw 1 *"
        yield "C tp @e[tag=lineDrawloop,score_lineDrawXrev=0] ~1 ~ ~"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AX lineDraw 1 *"
        yield "C tp @e[tag=lineDrawloop,score_lineDrawXrev_min=1] ~-1 ~ ~"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AX lineDraw 1 *"
        yield "C execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players operation AX lineDraw -= TMAJOR lineDraw"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players operation AX lineDraw += TDX lineDraw"
        // - diff on Y
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AY lineDraw 1 *"
        yield "C tp @e[tag=lineDrawloop,score_lineDrawYrev=0] ~ ~1 ~"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AY lineDraw 1 *"
        yield "C tp @e[tag=lineDrawloop,score_lineDrawYrev_min=1] ~ ~-1 ~"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AY lineDraw 1 *"
        yield "C execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players operation AY lineDraw -= TMAJOR lineDraw"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players operation AY lineDraw += TDY lineDraw"
        // - diff on Z
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AZ lineDraw 1 *"
        yield "C tp @e[tag=lineDrawloop,score_lineDrawZrev=0] ~ ~ ~1"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AZ lineDraw 1 *"
        yield "C tp @e[tag=lineDrawloop,score_lineDrawZrev_min=1] ~ ~ ~-1"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test AZ lineDraw 1 *"
        yield "C execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players operation AZ lineDraw -= TMAJOR lineDraw"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players operation AZ lineDraw += TDZ lineDraw"
        // endloop, stop when done after MAJOR steps
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players remove MAJOR lineDraw 1"
        yield "execute @e[tag=lineDrawloop] ~ ~ ~ scoreboard players test MAJOR lineDraw * -1"
        yield "C kill @e[tag=lineDrawloop]"
        // init bit
        yield "R"
        yield "O gamerule commandBlockOutput false"
        yield "scoreboard objectives add lineDraw dummy"
        yield "scoreboard objectives add lineDrawZrev dummy"
        yield "scoreboard objectives add lineDrawYrev dummy"
        yield "scoreboard objectives add lineDrawXrev dummy"
        yield "scoreboard players set @a lineDraw 0"
        yield """tellraw @a {"text":"Initializing, wait one moment...","color":"red"}"""
    |]

let cmds = 
    [|
        // computes the dx from A to B (provided it's in within -127...127) 
        let MAX = 127
        let STEPS = [64;32;16;8;4;2;1]
        yield "O "
        yield "scoreboard objectives add X dummy"
        yield "scoreboard players set @p X 0"
        yield sprintf "execute @e[tag=A] ~%d ~%d ~%d testfor @e[tag=B,dx=%d,dy=%d,dz=%d]" 0 (0-MAX) (0-MAX) 0 (2*MAX) (2*MAX)
        yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
        yield "C blockdata ~ ~-2 ~ {auto:1b}"
        yield "C blockdata ~ ~-1 ~ {auto:0b}"
        yield "O "
        yield "execute @e[tag=A] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"T\"]}"
        for s in STEPS do
            yield sprintf "execute @e[tag=T] ~%d ~%d ~%d testfor @e[tag=B,dx=%d,dy=%d,dz=%d]" 0 (0-MAX) (0-MAX) s (2*MAX) (2*MAX)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=T] ~%d ~%d ~%d" s 0 0
            yield sprintf "C scoreboard players add @p X %d" s
        yield "scoreboard players add @p X 1"  // always OBO 
        yield "kill @e[tag=T]"
        yield sprintf "scoreboard players test @p X %d *" (MAX+1)  // not found
        yield "C blockdata ~ ~-2 ~ {auto:1b}"
        yield "C blockdata ~ ~-1 ~ {auto:0b}"
        yield "O "
        let STEPS = [-64;-32;-16;-8;-4;-2;-1]
        yield "scoreboard players set @p X 0"
        yield "execute @e[tag=A] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"T\"]}"
        for s in STEPS do
            yield sprintf "execute @e[tag=T] ~%d ~%d ~%d testfor @e[tag=B,dx=%d,dy=%d,dz=%d]" 0 (0-MAX) (0-MAX) s (2*MAX) (2*MAX)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=T] ~%d ~%d ~%d" s 0 0
            yield sprintf "C scoreboard players remove @p X %d" (0-s)  // can't add negatives
        yield "scoreboard players remove @p X 1"  // always OBO 
        yield "kill @e[tag=T]"
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



