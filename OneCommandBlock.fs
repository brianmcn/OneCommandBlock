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

let drawSpiralCommonInit =
    [|
        "gamerule commandBlockOutput false"
        // since there will be different entities working at once in different places, we need lots of objectives, 1 for each variable of the circle algorithm
        // the objective variables are:
        // - XX        the 'radius' starter, init to R, sometimes --
        // - YY        the mover, starts at 0, always ++
        // - DD        the error approx, init 1-XX
        "scoreboard objectives add XX dummy"
        "scoreboard objectives add YY dummy"
        "scoreboard objectives add DD dummy"
        // register for parallel temp calcs
        "scoreboard objectives add Temp dummy"
        // also need way for player to trigger
        "scoreboard objectives add spiralDist dummy"   // how far out to spiral
    |]
(*
Step  Quarter circles radii Quarters center points 
(1+4i)° 2*d+b +8i*d (-d,d)                      q1
(2+4i)° 4*d+b +8i*d (-d,-d)                     q2
(3+4i)° 6*d+b +8i*d (d,-d)                      q3
(4+4i)° 8*d+b +8i*d (d,d)                       q4
  
Step  Quarter starting point Quarter end point 
(1+4i)° (d+b+8i*d,d) (-d,3*d+b+8i*d) 
(2+4i)° (-d,3*d+b+8i*d) (-5*d-b-8i*d,-d) 
(3+4i)° (-5*d-b-8i*d,-d) (d,-7*d-b-8i*d) 
(4+4i)° (d,-7*d-b-8i*d) (9*d+b+8i*d,d)
*)    
let drawSpiral =
    [|
        yield "P " 
        //////////////////////////////////////////
        // detect a scoreboard update and launch the mechanism
        //////////////////////////////////////////
        let DEE = 1
        let BEE = 1
        // init XX XX to first radius value of q1   (2*d+b +8i*d)
        yield sprintf "execute @p[score_spiralDist_min=3] ~ ~ ~ scoreboard players set XX XX %d" (2*DEE + BEE)  
        // init q1dropoff at first endpoint         (d+b+8i*d,d)
        yield sprintf "execute @p[score_spiralDist_min=3] ~ ~ ~ summon ArmorStand ~%d ~ ~%d {Marker:1,NoGravity:1,Tags:[\"q1dropoff\",\"alldropoff\"]}" (DEE+BEE) (-DEE)
        // init q2dropoff at second endpoint        (-d,3*d+b+8i*d) 
        yield sprintf "execute @p[score_spiralDist_min=3] ~ ~ ~ summon ArmorStand ~%d ~ ~%d {Marker:1,NoGravity:1,Tags:[\"q2dropoff\",\"alldropoff\"]}" (-DEE) (-3*DEE-BEE)
        // init q3dropoff at third endpoint         (-5*d-b-8i*d,-d)
        yield sprintf "execute @p[score_spiralDist_min=3] ~ ~ ~ summon ArmorStand ~%d ~ ~%d {Marker:1,NoGravity:1,Tags:[\"q3dropoff\",\"alldropoff\"]}" (-5*DEE-BEE) (DEE)
        // init q4dropoff at third endpoint         (d,-7*d-b-8i*d)
        yield sprintf "execute @p[score_spiralDist_min=3] ~ ~ ~ summon ArmorStand ~%d ~ ~%d {Marker:1,NoGravity:1,Tags:[\"q4dropoff\",\"alldropoff\"]}" (DEE) (7*DEE+BEE)
        // init CHOSEN spiralDist to @p spiralDist and zero out player
        yield "execute @p[score_spiralDist_min=3] ~ ~ ~ scoreboard players operation CHOSEN spiralDist = @p spiralDist"
        yield "scoreboard players set @p[score_spiralDist_min=3] spiralDist 0"
        //////////////////////////////////////////
        // drop off the q1 and q2 markers
        //////////////////////////////////////////
        yield "execute @e[tag=q1dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q1init\",\"allinit\"]}"
        yield "execute @e[tag=q2dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q1binit\",\"allinit\"]}"
        yield "execute @e[tag=q2dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q2init\",\"allinit\"]}"
        yield "execute @e[tag=q3dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q2binit\",\"allinit\"]}"
        yield "execute @e[tag=q3dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q3init\",\"allinit\"]}"
        yield "execute @e[tag=q4dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q3binit\",\"allinit\"]}"
        yield "execute @e[tag=q4dropoff] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"q4init\",\"allinit\"]}"
        yield sprintf "execute @e[tag=q1dropoff] ~ ~ ~ summon ArmorStand ~%d ~ ~ {Marker:1,NoGravity:1,Tags:[\"q4binit\",\"allinit\"]}" (8*DEE)
        yield "scoreboard players operation Temp XX = XX XX"
        yield "scoreboard players operation Temp XX -= CHOSEN spiralDist"
        yield "scoreboard players test Temp XX 0 *"  // TODO make so can end at any of 4 quadrants
        yield "C kill @e[tag=alldropoff]"
        // init the vars
        yield "scoreboard players operation @e[tag=allinit] XX = XX XX"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players add @e[tag=q2init] XX 2"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players add @e[tag=q2binit] XX 2"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players add @e[tag=q3init] XX 4"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players add @e[tag=q3binit] XX 4"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players add @e[tag=q4init] XX 6"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players add @e[tag=q4binit] XX 6"  // for q2/q3/q4, add 2/4/6
        yield "scoreboard players set @e[tag=allinit] YY 0"
        yield "scoreboard players set @e[tag=allinit] DD 1"
        yield "execute @e[tag=allinit] ~ ~ ~ scoreboard players operation @e[tag=allinit,c=1] DD -= @e[tag=allinit,c=1] XX"
        yield "entitydata @e[tag=q1init] {Tags:[\"ANZ\",\"SNX\",\"allrun\"]}"   // always negative Z, sometimes negative X
        yield "entitydata @e[tag=q1binit] {Tags:[\"APX\",\"SPZ\",\"allrun\"]}"  // always positive X, sometimes positive Z
        yield "entitydata @e[tag=q2init] {Tags:[\"ANX\",\"SPZ\",\"allrun\"]}"   // etc
        yield "entitydata @e[tag=q2binit] {Tags:[\"ANZ\",\"SPX\",\"allrun\"]}"
        yield "entitydata @e[tag=q3init] {Tags:[\"APZ\",\"SPX\",\"allrun\"]}"
        yield "entitydata @e[tag=q3binit] {Tags:[\"ANX\",\"SNZ\",\"allrun\"]}"
        yield "entitydata @e[tag=q4init] {Tags:[\"APX\",\"SNZ\",\"allrun\"]}"
        yield "entitydata @e[tag=q4binit] {Tags:[\"APZ\",\"SNX\",\"allrun\"]}"
        // iter the loop for next dropoff
        yield sprintf "scoreboard players add XX XX %d" (8 * DEE)
        yield sprintf "tp @e[tag=q1dropoff] ~%d ~ ~" (8 * DEE)
        yield sprintf "tp @e[tag=q2dropoff] ~ ~ ~%d" (-8 * DEE)
        yield sprintf "tp @e[tag=q3dropoff] ~%d ~ ~" (-8 * DEE)
        yield sprintf "tp @e[tag=q4dropoff] ~ ~ ~%d" (8 * DEE)

        // debug
        for prefix in ["q1";"q1b";"q2";"q2b";"q3";"q3b";"q4";"q4b"] do
            yield sprintf "scoreboard players operation FAKE XX = @e[tag=%srun,c=1] XX" prefix
            yield sprintf "scoreboard players operation FAKE YY = @e[tag=%srun,c=1] YY" prefix
            yield sprintf "scoreboard players operation FAKE DD = @e[tag=%srun,c=1] DD" prefix
            yield sprintf """execute @e[tag=%srun,c=1] ~ ~ ~ tellraw @a ["%s  XX=",{"score":{"name":"FAKE","objective":"XX"}},"  YY=",{"score":{"name":"FAKE","objective":"YY"}},"  DD=",{"score":{"name":"FAKE","objective":"DD"}}]""" prefix prefix

        // run
        // while Y <= X
        yield "execute @e[tag=allrun] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] Temp = @e[tag=allrun,c=1] YY"
        yield "execute @e[tag=allrun] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] Temp -= @e[tag=allrun,c=1] XX"
        yield "kill @e[tag=allrun,score_Temp_min=1]"
        // Plot
        yield "execute @e[tag=allrun] ~ ~ ~ setblock ~ ~ ~ stone"  // TODO block
        // Y--
        yield "tp @e[tag=ANZ] ~ ~ ~-1"
        yield "tp @e[tag=APX] ~1 ~ ~"
        yield "tp @e[tag=ANX] ~-1 ~ ~"
        yield "tp @e[tag=APZ] ~ ~ ~1"
        yield "scoreboard players add @e[tag=allrun] YY 1"
        // if D < 0 then D += 2Y+1
        // else x--, D += 2(Y-X)+1
        // ...aka, always add 2Y+1, if >0, X-- and subtract 2X
        yield "execute @e[tag=allrun] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] DD += @e[tag=allrun,c=1] YY"
        yield "execute @e[tag=allrun] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] DD += @e[tag=allrun,c=1] YY"
        yield "scoreboard players add @e[tag=allrun] DD 1"
        yield "scoreboard players remove @e[tag=allrun,score_DD_min=1] XX 1"
        yield "tp @e[tag=SNX,score_DD_min=1] ~-1 ~ ~"
        yield "tp @e[tag=SPZ,score_DD_min=1] ~ ~ ~1"
        yield "tp @e[tag=SPX,score_DD_min=1] ~1 ~ ~"
        yield "tp @e[tag=SNZ,score_DD_min=1] ~ ~ ~-1"
        yield "execute @e[tag=allrun,score_DD_min=1] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] DD -= @e[tag=allrun,c=1] XX"
        yield "execute @e[tag=allrun,score_DD_min=1] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] DD -= @e[tag=allrun,c=1] XX"
    |]


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODO below does not work if '^' is in the original text
let escape(s:string) = s.Replace("\"","^").Replace("\\","\\\\").Replace("^","\\\"")    //    "  \    ->    \"   \\
let escape2(s) = escape(escape(s))

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

let makeBlockAsCommandRelative(blockCmd,dx,dy,dz) =
    if blockCmd = "R" then
        sprintf "setblock ~%d ~%d ~%d redstone_block" dx dy dz
    elif blockCmd.StartsWith("P ") then
        sprintf "setblock ~%d ~%d ~%d repeating_command_block 0 replace {auto:1b,Command:\"%s\"}" dx dy dz (escape (blockCmd.Substring(2)))
    elif blockCmd.StartsWith("O ") then
        sprintf "setblock ~%d ~%d ~%d command_block 0 replace {Command:\"%s\"}" dx dy dz (escape (blockCmd.Substring(2)))
    elif blockCmd.StartsWith("C ") then
        sprintf "setblock ~%d ~%d ~%d chain_command_block 8 replace {auto:1b,Command:\"%s\"}" dx dy dz (escape (blockCmd.Substring(2)))
    else 
        sprintf "setblock ~%d ~%d ~%d chain_command_block 0 replace {auto:1b,Command:\"%s\"}" dx dy dz (escape blockCmd)

let minecartCommandRunner(cmds:string[]) =
    let sb = new System.Text.StringBuilder()
    // runs all cmds in 1 tick, 8 ticks after activation, leaves only original block
    sb.Append(sprintf """summon FallingSand ~ ~0.55 ~ {Block:command_block,Time:1,TileEntityData:{Command:"summon MinecartCommandBlock ~ ~1.5 ~ {Command:\"%s\",Passengers:[""" (escape2 cmds.[0])) |> ignore
    for i = 1 to cmds.Length-1 do
        sb.Append(sprintf """{id:MinecartCommandBlock,Command:\"%s\"},""" (escape2 cmds.[i])) |> ignore
    sb.Append("""{id:MinecartCommandBlock,Command:\"setblock ~ ~-2 ~ command_block 0 0 {auto:1b,Command:\\\"fill ~ ~ ~ ~ ~2 ~ air\\\"}\"},{id:MinecartCommandBlock,Command:\"kill @e[type=MinecartCommandBlock,r=5]\"}]}"},Passengers:[{id:FallingSand,Block:redstone_block,Time:1,Passengers:[{id:FallingSand,Block:activator_rail,Time:1}]}]}""") |> ignore
    sb.ToString()

// first run all runCmds, then place each blockCmds array hanging down in the sky, starting at same y/z but with increasing x
let makeOneCommandBlockWith(runCmds:string[], blockCmds:string[][]) =
    let finalCmds = ResizeArray()
    finalCmds.AddRange(runCmds)
    let maxLen = blockCmds |> Array.map (fun a -> a.Length) |> Array.max
    for x = 0 to blockCmds.Length-1 do
        for i = 0 to blockCmds.[x].Length-1 do
            let cmd = makeBlockAsCommandRelative(blockCmds.[x].[i], x*2, 2 + maxLen - i, 0)
            finalCmds.Add(cmd)
    minecartCommandRunner(finalCmds.ToArray())

let makeOneCommandBlock(cmds:string[]) =
    let mutable sb = new System.Text.StringBuilder()
    if not(cmds.[0].StartsWith("MINECART")) then
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
                sb.Append(sprintf "{id:FallingSand,Block:command_block,Time:1,TileEntityData:{Command:\"%s\"},Passengers:[" (escape2 (c.Substring(2)))) |> ignore
            elif c.StartsWith("C ") then
                sb.Append(sprintf "{id:FallingSand,Block:chain_command_block,Time:1,Data:8,TileEntityData:{Command:\"%s\"},Passengers:[" (escape2 (c.Substring(2)))) |> ignore
            else
                sb.Append(sprintf "{id:FallingSand,Block:chain_command_block,Time:1,TileEntityData:{Command:\"%s\"},Passengers:[" (escape2 c)) |> ignore
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
        if cmds.[0].StartsWith("MINECART BLOCKS") then
            for i = 1 to cmds.Length-1 do
                let dy = 10 + cmds.Length - i 
                cmds.[i] <- makeBlockAsCommandRelative(cmds.[i],0,dy,0)
        sb.Append(minecartCommandRunner(cmds.[1..])) |> ignore
#endif
    sb.ToString()

[<System.STAThreadAttribute>]
do
(*
    let s = makeOneCommandBlockWith([|"say run 1";"say run 2";"say run 3"|],
                                    [| [|"P";"O say 1";"C say 1";"say 1";"R"|] 
                                       [|"P";"O say 2";"C say 2";"say 2";"R"|] 
                                    |])
*)
    let a,b = Geometry.All
    let s = makeOneCommandBlockWith(a,b)
    let s = makeOneCommandBlock(OldContraptions.drawCircle)
    let s = makeOneCommandBlockWith(drawSpiralCommonInit,[|drawSpiral|])
    System.Windows.Clipboard.SetText(s)
    printfn "%s" (s)
    printfn ""
    printfn "left  %d" (s |> Seq.filter (fun c -> c='{') |> Seq.length)
    printfn "right %d" (s |> Seq.filter (fun c -> c='}') |> Seq.length)
    printfn "chars %d" (s.Length)
    //printfn "blocks %d" (cmds.Length)
    printfn ""
    printfn "it's now in your clipboard"



