module OldContraptions

let nudgeZ =
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

//////////////////////////////////////////////////////////////////////


let rand = new System.Random()

let swap (a: _[]) x y =
    let tmp = a.[x]
    a.[x] <- a.[y]
    a.[y] <- tmp

let shuffle a = Array.iteri (fun i _ -> swap a i (rand.Next(i, Array.length a))) a

let computeSphereShellAndInterior() =
    let mutable THRESHOLD = 13  // 13 barely encloses -2..2 cube, 28 barely encloses -3..3 cube
    let a = Array3D.create 19 19 19 false
    for x = -9 to 9 do
        for y = -9 to 9 do
            for z = -9 to 9 do
                if x*x + y*y + z*z < THRESHOLD then
                    a.[10+x,10+y,10+z] <- true
    // now a holds the solid sphere; the shell is all cells with 'air' next to them
    let shell = ResizeArray()
    for x = -8 to 8 do
        for y = -8 to 8 do
            for z = -8 to 8 do
                if a.[10+x,10+y,10+z] then
                    if not a.[11+x,10+y,10+z] || not a.[10+x,11+y,10+z]|| not a.[10+x,10+y,11+z]
                        || not a.[9+x,10+y,10+z] || not a.[10+x,9+y,10+z]|| not a.[10+x,10+y,9+z] then
                        // one face is next to air, is shell
                        shell.Add( (x,y,z) )
    let interior = ResizeArray()
    for x = -8 to 8 do
        for y = -8 to 8 do
            for z = -8 to 8 do
                if a.[10+x,10+y,10+z] then
                    if not(shell.Contains( (x,y,z) )) then
                        if not(x=0 && y=0 && z=0) then   // exclude center, we're putting fence_gate there
                            interior.Add( (x,y,z) )
    shell, interior
let shell, interior = computeSphereShellAndInterior()                    
let XXX = 6  // noise frequency = 1/XXX
let a = shell |> Seq.toArray 
shuffle a
let b = a |> Array.mapi (fun i (x,y,z) -> x,y,z,i%XXX)
let noisyTunneler =
    [|
        yield "R"
        yield """O tellraw @a {"text":"Set 'Dig' score to 1 to dig, 0 to not","color":"green"}"""
        yield "R"
        yield "P"
        yield "scoreboard players add @p S 1"
        yield (sprintf "scoreboard players set @p[score_S_min=%d] S 0" XXX)
        for x,y,z,n in b do
            yield (sprintf "execute @p[score_Dig_min=1,score_S_min=%d,score_S=%d] ~ ~ ~ detect ~ ~ ~ air 0 setblock ~%d ~%d ~%d air" n n x y z)
        yield "execute @p[score_Dig_min=1] ~ ~ ~ detect ~ ~ ~ air 0 fill ~-2 ~-1 ~-1 ~2 ~1 ~1 air"
        yield "execute @p[score_Dig_min=1] ~ ~ ~ detect ~ ~ ~ air 0 fill ~-1 ~-2 ~-1 ~1 ~2 ~1 air"
        yield "execute @p[score_Dig_min=1] ~ ~ ~ detect ~ ~ ~ air 0 fill ~-1 ~-1 ~-2 ~1 ~1 ~2 air"
        yield "execute @p[score_Dig_min=1] ~ ~ ~ setblock ~ ~ ~ fence_gate 5"
        yield "execute @p[score_Dig=0] ~ ~ ~ detect ~ ~ ~ fence_gate 5 setblock ~ ~ ~ air"
        yield "R"
        yield "O gamerule commandBlockOutput false"
        yield "scoreboard objectives add Dig dummy"
        yield "scoreboard objectives add S dummy"
        yield "scoreboard objectives setdisplay sidebar Dig"
        yield "scoreboard players set @p Dig 0"
        yield """tellraw @a {"text":"Initializing, wait one moment...","color":"red"}"""
    |]    


///////////////////////////////////////////////////////////////////

let drawCircle =
    [|
        yield "R"
        yield """O tellraw @a {"text":"'circleY' by Dr. Brian Lorgon111","color":"yellow"}"""
        yield """tellraw @a {"text":"Type '/scoreboard players set @p circleY NNN' to make a circle of stone with radius NNN, centered at yourself. The circle is in the Y-plane, and NNN must be at least 3.","color":"green"}"""
        yield "R"
        yield "P"
        // detect a scoreboard update and launch the mechanism
        yield "execute @p[score_circleY_min=3] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYstart\"],Marker:1,NoGravity:1}"
        yield "execute @p[score_circleY_min=3] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYstart2\"],Marker:1,NoGravity:1}"
        yield "execute @p[score_circleY_min=3] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYstart3\"],Marker:1,NoGravity:1}"
        yield "execute @p[score_circleY_min=3] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYstart4\"],Marker:1,NoGravity:1}"
        yield "C scoreboard players operation X circleY = @p[score_circleY_min=3] circleY"
        yield "C scoreboard players set Y circleY 0"
        yield "C scoreboard players set D circleY 1"
        yield "C scoreboard players operation D circleY -= X circleY"
        yield "C scoreboard players operation @e[tag=circleYstart] circleY = @p[score_circleY_min=3] circleY"
        yield "C scoreboard players set @a[score_circleY_min=3] circleY 0"
        // go N spaces along +Z axis
        yield "tp @e[tag=circleYstart,score_circleY_min=1] ~ ~ ~1"
        yield "C tp @e[tag=circleYstart2] ~1 ~ ~"
        yield "C tp @e[tag=circleYstart3] ~ ~ ~-1"
        yield "C tp @e[tag=circleYstart4] ~-1 ~ ~"
        yield "scoreboard players remove @e[tag=circleYstart,score_circleY_min=1] circleY 1"
        // once there, do alg
        yield "execute @e[tag=circleYstart,score_circleY=0] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover\"],Marker:1,NoGravity:1}"
        yield "execute @e[tag=circleYstart,score_circleY=0] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmoverb\"],Marker:1,NoGravity:1}"
        yield "C execute @e[tag=circleYstart2] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover2\"],Marker:1,NoGravity:1}"
        yield "C execute @e[tag=circleYstart2] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover2b\"],Marker:1,NoGravity:1}"
        yield "C execute @e[tag=circleYstart3] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover3\"],Marker:1,NoGravity:1}"
        yield "C execute @e[tag=circleYstart3] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover3b\"],Marker:1,NoGravity:1}"
        yield "C execute @e[tag=circleYstart4] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover4\"],Marker:1,NoGravity:1}"
        yield "C execute @e[tag=circleYstart4] ~ ~ ~ summon ArmorStand ~ ~ ~ {Tags:[\"circleYdraw\",\"circleYmover4b\"],Marker:1,NoGravity:1}"
        yield "kill @e[tag=circleYstart,score_circleY=0]"
        yield "C kill @e[tag=circleYstart2]"
        yield "C kill @e[tag=circleYstart3]"
        yield "C kill @e[tag=circleYstart4]"
        // while Y <= X
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players operation Temp circleY = Y circleY"
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players operation Temp circleY -= X circleY"
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players test Temp circleY 1 *"
        yield "C kill @e[tag=circleYdraw]"
        // do alg
        yield "execute @e[tag=circleYdraw] ~ ~ ~ setblock ~ ~ ~ stone"  // TODO block
        yield "tp @e[tag=circleYmover] ~1 ~ ~"
        yield "tp @e[tag=circleYmoverb] ~-1 ~ ~"
        yield "tp @e[tag=circleYmover2] ~ ~ ~1"
        yield "tp @e[tag=circleYmover2b] ~ ~ ~-1"
        yield "tp @e[tag=circleYmover3] ~-1 ~ ~"
        yield "tp @e[tag=circleYmover3b] ~1 ~ ~"
        yield "tp @e[tag=circleYmover4] ~ ~ ~-1"
        yield "tp @e[tag=circleYmover4b] ~ ~ ~1"
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players add Y circleY 1"
        // if D < 0 then D += 2Y+1
        // else x--, D += 2(Y-X)+1
        // ...aka, always add 2Y+1, if >0, X-- and subtract 2X
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players operation D circleY += Y circleY"
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players operation D circleY += Y circleY"
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players add D circleY 1"
        yield "execute @e[tag=circleYmover] ~ ~ ~ scoreboard players test D circleY 1 *"
        yield "C scoreboard players remove X circleY 1"
        yield "C tp @e[tag=circleYmover] ~ ~ ~-1"
        yield "C tp @e[tag=circleYmoverb] ~ ~ ~-1"
        yield "C tp @e[tag=circleYmover2] ~-1 ~ ~"
        yield "C tp @e[tag=circleYmover2b] ~-1 ~ ~"
        yield "C tp @e[tag=circleYmover3] ~ ~ ~1"
        yield "C tp @e[tag=circleYmover3b] ~ ~ ~1"
        yield "C tp @e[tag=circleYmover4] ~1 ~ ~"
        yield "C tp @e[tag=circleYmover4b] ~1 ~ ~"
        yield "C scoreboard players operation D circleY -= X circleY"
        yield "C scoreboard players operation D circleY -= X circleY"
        // init bit
        yield "R"
        yield "O gamerule commandBlockOutput false"
        yield "scoreboard objectives add circleY dummy"
        yield "scoreboard players set @a circleY 0"
        yield """tellraw @a {"text":"Initializing, wait one moment...","color":"red"}"""
    |]    

///////////////////////////////////////////////////////////////////

