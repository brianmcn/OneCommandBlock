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


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODO below does not work if '^' is in the original text
let escape(s:string) = s.Replace("\"","^").Replace("\\","\\\\").Replace("^","\\\"")    //    "  \    ->    \"   \\
let escape2(s) = escape(escape(s))

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

let makeBlockAsCommandRelative(blockCmd,dx,dy,dz) =
    if blockCmd = "R" then
        sprintf "setblock ~%d ~%d ~%d redstone_block" dx dy dz
    elif blockCmd = "P" then
        sprintf "setblock ~%d ~%d ~%d repeating_command_block" dx dy dz
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
//    let s = makeOneCommandBlock(cmds)
(*
    let s = makeOneCommandBlockWith([|"say run 1";"say run 2";"say run 3"|],
                                    [| [|"P";"O say 1";"C say 1";"say 1";"R"|] 
                                       [|"P";"O say 2";"C say 2";"say 2";"R"|] 
                                    |])
*)
    let a,b = Geometry.All
    let s = makeOneCommandBlockWith(a,b)
    System.Windows.Clipboard.SetText(s)
    printfn "%s" (s)
    printfn ""
    printfn "left  %d" (s |> Seq.filter (fun c -> c='{') |> Seq.length)
    printfn "right %d" (s |> Seq.filter (fun c -> c='}') |> Seq.length)
    printfn "chars %d" (s.Length)
    //printfn "blocks %d" (cmds.Length)
    printfn ""
    printfn "it's now in your clipboard"



