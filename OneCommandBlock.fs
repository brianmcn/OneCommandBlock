let cmds = 
    [|
        """R"""
        """O give @p minecraft:spawn_egg 1 0 {EntityTag:{id:Bat,NoAI:1,Silent:1,Tags:["nudgeZ"]},display:{Name:"nudgeZ"}}"""
        """tellraw @a {"text":"'nudgeZ' by Dr. Brian Lorgon111","color":"yellow"}"""
        """tellraw @a ["type '",{"text":"/gamerule help-nudgeZ 111","color":"green"},"' to get help"]"""
        """gamerule help-nudgeZ 999"""
        """scoreboard objectives add nudgeZ dummy"""
        """scoreboard players set nudgeZ nudgeZ 999"""
        """stats block ~ ~-2 ~ set QueryResult nudgeZ nudgeZ"""
        """P"""
        """gamerule help-nudgeZ"""
        """scoreboard players test nudgeZ nudgeZ 111 111"""
        """C gamerule help-nudgeZ 999"""
        """C give @p minecraft:spawn_egg 1 0 {EntityTag:{id:Bat,NoAI:1,Silent:1,Tags:["nudgeZ"]},display:{Name:"nudgeZ"}}"""
        """C say You've been given a useful spawn egg!"""
        """C say Use the egg on top of most any block ('nudge') to nudge that block and its followers +1 in the Z direction."""
        """C say Use the egg on top of a REDSTONE block to delete the redstone block and nudge the following blocks -1 in the Z direction."""
        """C say Useful for inserting an air space into the middle of a +Z-pointing command-chain (nudge the block you want to insert before), or for deleting a block from that chain (destroy block, place redstone in its place, and nudge the redstone)."""
        """execute @e[tag=nudgeZ] ~ ~ ~ detect ~ ~-1 ~ redstone_block 0 summon ArmorStand ~ ~-1 ~ {NoGravity:1,Tags:["end-Z","doZ"]}"""
        """execute @e[tag=nudgeZ] ~ ~ ~ detect ~ ~-1 ~ redstone_block 0 summon ArmorStand ~ ~-1 ~ {NoGravity:1,Tags:["findstart-Z","doZ"]}"""
        """execute @e[tag=end-Z] ~ ~ ~ kill @e[tag=nudgeZ]"""
        """execute @e[tag=findstart-Z] ~ ~ ~ detect ~ ~ ~ air 0 entitydata @e[c=1] {Tags:["foundstart-Z","doZ"]}"""
        """tp @e[tag=findstart-Z] ~ ~ ~1"""
        """execute @e[tag=end-Z] ~ ~ ~ execute @e[r=0,tag=foundstart-Z] ~ ~ ~ setblock ~ ~ ~ air"""
        """execute @e[tag=end-Z] ~ ~ ~ execute @e[r=0,tag=foundstart-Z] ~ ~ ~ kill @e[tag=doZ]"""
        """execute @e[tag=foundstart-Z] ~ ~ ~ execute @e[tag=end-Z] ~ ~ ~ clone ~ ~ ~1 ~ ~ ~1 ~ ~ ~"""
        """execute @e[tag=foundstart-Z] ~ ~ ~ tp @e[tag=end-Z] ~ ~ ~1"""
        """execute @e[tag=findstart-Z] ~ ~ ~ testfor @e[tag=end-Z,r=50]"""
        """testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"""
        """C execute @e[tag=findstart-Z] ~ ~ ~ kill @e[tag=doZ]"""
        """C say failed to nudge (more than 50 blocks before air)"""
        """execute @e[tag=nudgeZ] ~ ~ ~ summon ArmorStand ~ ~-1 ~ {NoGravity:1,Tags:["startZ","doZ"]}"""
        """execute @e[tag=nudgeZ] ~ ~ ~ summon ArmorStand ~ ~-1 ~ {NoGravity:1,Tags:["findendZ","doZ"]}"""
        """kill @e[tag=nudgeZ]"""
        """execute @e[tag=findendZ] ~ ~ ~ detect ~ ~ ~ air 0 entitydata @e[c=1] {Tags:["foundendZ","doZ"]}"""
        """tp @e[tag=findendZ] ~ ~ ~1"""
        """execute @e[tag=foundendZ] ~ ~ ~ execute @e[r=0,tag=startZ] ~ ~ ~ setblock ~ ~ ~ air"""
        """execute @e[tag=foundendZ] ~ ~ ~ execute @e[r=0,tag=startZ] ~ ~ ~ kill @e[tag=doZ]"""
        """execute @e[tag=foundendZ] ~ ~ ~ clone ~ ~ ~-1 ~ ~ ~-1 ~ ~ ~"""
        """tp @e[tag=foundendZ] ~ ~ ~-1"""
        """execute @e[tag=findendZ] ~ ~ ~ testfor @e[tag=startZ,r=50]"""
        """testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"""
        """C execute @e[tag=findendZ] ~ ~ ~ kill @e[tag=doZ]"""
        """C say failed to nudge (more than 50 blocks before air)"""
        """R"""
        """O gamerule commandBlockOutput false"""
        """tellraw @a {"text":"Initializing, wait one moment...","color":"red"}"""
    |]

let escape(s:string) = s.Replace("\"","\\\"")

[<System.STAThreadAttribute>]
do
    let sb = new System.Text.StringBuilder()
    sb.Append("""summon Item ~ ~10 ~ {Item:{id:dirt,Damage:0,Count:1},Age:5999,""") |> ignore
    for c in cmds do
        if c = "R" then
            sb.Append("Riding:{id:FallingSand,Block:redstone_block,Time:1,") |> ignore 
        elif c = "P" then
            sb.Append("Riding:{id:FallingSand,Block:repeating_command_block,Time:1,TileEntityData:{auto:1b},") |> ignore
        elif c.StartsWith("O ") then
            sb.Append(sprintf "Riding:{id:FallingSand,Block:command_block,Time:1,TileEntityData:{Command:\"%s\"}," (escape (c.Substring(2)))) |> ignore
        elif c.StartsWith("C ") then
            sb.Append(sprintf "Riding:{id:FallingSand,Block:chain_command_block,Time:1,Data:8,TileEntityData:{Command:\"%s\"}," (escape (c.Substring(2)))) |> ignore
        else
            sb.Append(sprintf "Riding:{id:FallingSand,Block:chain_command_block,Time:1,TileEntityData:{Command:\"%s\"}," (escape c)) |> ignore
    let sb = new System.Text.StringBuilder(sb.ToString().Substring(0,sb.Length-1))  // strip extra comma
    for i = 0 to cmds.Length do
        sb.Append("""}""") |> ignore

    System.Windows.Clipboard.SetText(sb.ToString())
    printfn "%s" (sb.ToString())
    printfn ""
    printfn "%d" (sb.ToString() |> Seq.filter (fun c -> c='{') |> Seq.length)
    printfn "%d" (sb.ToString() |> Seq.filter (fun c -> c='}') |> Seq.length)
    printfn "%d" (sb.ToString().Length)
    printfn ""
    printfn "it's now in your clipboard"

