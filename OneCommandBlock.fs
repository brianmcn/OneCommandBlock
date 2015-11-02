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
        "scoreboard objectives add spiralDist dummy"   // how far out to spiral, also used to count quadrants
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
#if FLAT
        let DY = 0
#else
        let DY = 1
#endif
#if TIGHT
        let POFF = [| 0; 1; 2; 3 |]
        let RFACTOR = 1
#else
        let POFF = [| 1; 3; 5; 7 |]
        let RFACTOR = 2
#endif
        let ITER = RFACTOR * 4

        let START_R = (RFACTOR*DEE + BEE)  
//(*
        // init a count of how many quadrants we're through
        yield sprintf "execute @p[score_spiralDist_min=1] ~ ~ ~ scoreboard players set COUNT spiralDist 1"
//*)
        // init XX XX to first radius value of q1   (2*d+b +8i*d)
        yield sprintf "execute @p[score_spiralDist_min=1] ~ ~ ~ scoreboard players set XX XX %d" START_R
        // init q1dropoff at first endpoint         (d+b+8i*d,d)
        yield sprintf "execute @p[score_spiralDist_min=1] ~ ~ ~ summon ArmorStand ~%d ~%d ~%d {Marker:1,NoGravity:1,Tags:[\"q1dropoff\",\"alldropoff\"]}" (POFF.[0]*DEE+BEE) 0 (-DEE)
        // init q2dropoff at second endpoint        (-d,3*d+b+8i*d) 
        yield sprintf "execute @p[score_spiralDist_min=1] ~ ~ ~ summon ArmorStand ~%d ~%d ~%d {Marker:1,NoGravity:1,Tags:[\"q2dropoff\",\"alldropoff\"]}" (-DEE) 0 (-POFF.[1]*DEE-BEE)
        // init q3dropoff at third endpoint         (-5*d-b-8i*d,-d)
        yield sprintf "execute @p[score_spiralDist_min=1] ~ ~ ~ summon ArmorStand ~%d ~%d ~%d {Marker:1,NoGravity:1,Tags:[\"q3dropoff\",\"alldropoff\"]}" (-POFF.[2]*DEE-BEE) 0 (DEE)
        // init q4dropoff at third endpoint         (d,-7*d-b-8i*d)
        yield sprintf "execute @p[score_spiralDist_min=1] ~ ~ ~ summon ArmorStand ~%d ~%d ~%d {Marker:1,NoGravity:1,Tags:[\"q4dropoff\",\"alldropoff\"]}" (DEE) 0 (POFF.[3]*DEE+BEE)
        // init CHOSEN spiralDist to @p spiralDist and zero out player
        yield "execute @p[score_spiralDist_min=1] ~ ~ ~ scoreboard players operation CHOSEN spiralDist = @p spiralDist"
        yield "scoreboard players set @p[score_spiralDist_min=1] spiralDist 0"
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
        yield sprintf "execute @e[tag=q1dropoff] ~ ~ ~ summon ArmorStand ~%d ~ ~ {Marker:1,NoGravity:1,Tags:[\"q4binit\",\"allinit\"]}" (ITER*DEE)
        yield "scoreboard players operation Temp XX = XX XX"
        yield "scoreboard players operation Temp XX -= CHOSEN spiralDist"
        yield "scoreboard players test Temp XX 0 *"  // TODO make so can end at any of 4 quadrants
        yield "C kill @e[tag=alldropoff]"
        // init the vars
        yield "scoreboard players operation @e[tag=allinit] XX = XX XX"  // for q2/q3/q4, add 2/4/6
        yield sprintf "scoreboard players add @e[tag=q2init] XX %d"  (RFACTOR)
        yield sprintf "scoreboard players add @e[tag=q2binit] XX %d" (RFACTOR)
        yield sprintf "scoreboard players add @e[tag=q3init] XX %d"  (RFACTOR*2)
        yield sprintf "scoreboard players add @e[tag=q3binit] XX %d" (RFACTOR*2)
        yield sprintf "scoreboard players add @e[tag=q4init] XX %d"  (RFACTOR*3)
        yield sprintf "scoreboard players add @e[tag=q4binit] XX %d" (RFACTOR*3)
        yield "scoreboard players set @e[tag=allinit] YY 0"
        yield "scoreboard players set @e[tag=allinit] DD 1"
//(*
        yield "scoreboard players operation @e[tag=allinit] spiralDist = COUNT spiralDist"
        yield sprintf "scoreboard players add @e[tag=q1binit] spiralDist 1"
        yield sprintf "scoreboard players add @e[tag=q2init] spiralDist 2"
        yield sprintf "scoreboard players add @e[tag=q2binit] spiralDist 3"
        yield sprintf "scoreboard players add @e[tag=q3init] spiralDist 4"
        yield sprintf "scoreboard players add @e[tag=q3binit] spiralDist 5"
        yield sprintf "scoreboard players add @e[tag=q4init] spiralDist 6"
        yield sprintf "scoreboard players add @e[tag=q4binit] spiralDist 7"
//*)
        yield "execute @e[tag=allinit] ~ ~ ~ scoreboard players operation @e[tag=allinit,c=1] DD -= @e[tag=allinit,c=1] XX"
        yield "entitydata @e[tag=q1init] {Tags:[\"ANZ\",\"SNX\",\"FWD\",\"allrun\"]}"   // always negative Z, sometimes negative X, forward
        yield "entitydata @e[tag=q1binit] {Tags:[\"APX\",\"SPZ\",\"BWD\",\"allrun\"]}"  // always positive X, sometimes positive Z, backward
        yield "entitydata @e[tag=q2init] {Tags:[\"ANX\",\"SPZ\",\"FWD\",\"allrun\"]}"   // etc
        yield "entitydata @e[tag=q2binit] {Tags:[\"ANZ\",\"SPX\",\"BWD\",\"allrun\"]}"
        yield "entitydata @e[tag=q3init] {Tags:[\"APZ\",\"SPX\",\"FWD\",\"allrun\"]}"
        yield "entitydata @e[tag=q3binit] {Tags:[\"ANX\",\"SNZ\",\"BWD\",\"allrun\"]}"
        yield "entitydata @e[tag=q4init] {Tags:[\"APX\",\"SNZ\",\"FWD\",\"allrun\"]}"
        yield "entitydata @e[tag=q4binit] {Tags:[\"APZ\",\"SNX\",\"BWD\",\"allrun\"]}"
        // iter the loop for next dropoff
        yield sprintf "scoreboard players add COUNT spiralDist 8"
        yield sprintf "scoreboard players add XX XX %d" (ITER * DEE)
        yield sprintf "tp @e[tag=q1dropoff] ~%d ~%d ~%d" (ITER * DEE) 0 0
        yield sprintf "tp @e[tag=q2dropoff] ~%d ~%d ~%d" 0 0 (-ITER * DEE)
        yield sprintf "tp @e[tag=q3dropoff] ~%d ~%d ~%d" (-ITER * DEE) 0 0
        yield sprintf "tp @e[tag=q4dropoff] ~%d ~%d ~%d" 0 0 (ITER * DEE)

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
#if FLAT
        yield "execute @e[tag=allrun] ~ ~ ~ setblock ~ ~ ~ stone"  // TODO block
#else
        for i = 1 to 30 do
            yield sprintf "execute @e[tag=allrun,score_spiralDist=%d,score_spiralDist_min=%d] ~ ~ ~ setblock ~ ~%d ~ stone" i i i  // TODO block
(*
        yield """execute @e[tag=allrun] ~ ~ ~ setblock ~ ~ ~ command_block 0 replace {auto:1b,Command:"summon ArmorStand ~ ~ ~ {NoGravity:1,Duration:300,Tags:[\"newAEC\"]}"}"""
        yield "execute @e[tag=allrun,c=1] ~ ~ ~ scoreboard players set G XX 0"  // constantly set to 0 while still running
        yield "scoreboard players test G XX 1 *"  // check if done summoning all AECs, if so...
        for r in [4;8;9;10;11;12;16;20;24] do
            yield sprintf "C execute @p ~ ~ ~ execute @e[tag=newAEC,r=%d] ~ ~ ~ setblock ~ ~ ~ stone" r
            yield sprintf "C execute @p ~ ~ ~ kill @e[tag=newAEC,r=%d]" r
            yield sprintf "C tp @e[tag=newAEC] ~ ~%d ~" DY  // tp everyone else (if we're into 'next guy' stage)
            yield sprintf "C tp @p ~ ~%d ~" DY  // tp everyone else (if we're into 'next guy' stage)
        yield "scoreboard players add G XX 1"  // be able to detect when done using this
*)
(*
        yield "execute @e[tag=allrun] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Duration:300,Tags:[\"newAEC\"]}"  // Give the AECs an appropriate duration to last long enough to do work, but then go away.
        yield "execute @e[tag=allrun,c=1] ~ ~ ~ scoreboard players set G XX 0"  // constantly set to 0 while still running
        yield "scoreboard players test G XX 1 1"  // check if just got done summoning all AECs, if so...
        yield sprintf "C execute @p ~ ~ ~ scoreboard players tag @e[tag=newAEC,c=1] add nextAEC" // pick first guy
        yield sprintf "execute @e[tag=nextAEC] ~ ~ ~ setblock ~ ~ ~ stone"  // setblock him
//        yield """execute @e[tag=newAEC,score_XX_min=1] ~ ~ ~ tellraw @a [{"score":{"name":"G","objective":"XX"}}]"""
        yield sprintf "entitydata @e[tag=nextAEC] {Tags:[\"oldAEC\"]}"
        yield sprintf "execute @e[tag=oldAEC] ~ ~ ~ scoreboard players tag @e[tag=newAEC,c=1] add nextAEC"  // pick next guy
        yield sprintf "tp @e[tag=newAEC] ~ ~%d ~" DY  // tp everyone else (if we're into 'next guy' stage)
        yield sprintf "kill @e[tag=oldAEC]" // kill guy we just setblock'd
        yield "scoreboard players add G XX 1"  // be able to detect when done using this
*)
(*
        //instead of setblock, summon an AEC, give a score (YY + init offset, or init offset - YY for bs, where init offsets overshoot, just to preserve order)
        //  - each quadrant has about 1.414 R blocks, so 1.5*q4init's XX is a suitable offset spacing.  it is a lot of time, oh well
        yield """execute @e[tag=allrun] ~ ~ ~ setblock ~ ~ ~ command_block 0 replace {auto:1b,Command:"summon AreaEffectCloud ~ ~ ~ {NoGravity:1,Duration:300,Tags:[\"newAEC\"]}"}"""
//        yield "execute @e[tag=allrun] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Duration:500,Tags:[\"newAEC\"]}"  // Give the AECs an appropriate duration to last long enough to do work, but then go away.
//        yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players set @e[tag=newAEC] XX 0"
        yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players operation @e[tag=newAEC,c=1] XX = CHOSEN spiralDist"   // only init furthest in this block, to...
//        yield "kill @e[tag=newAEC,score_XX=0]" // ... remove duplicates
        yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players operation @e[tag=newAEC,c=1] XX += CHOSEN spiralDist"  
        yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players operation @e[tag=newAEC,c=1] XX += CHOSEN spiralDist"  // triple it, to overshoot
        yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players operation @e[tag=newAEC,c=1] XX *= @e[tag=allrun,c=1] spiralDist"  // multiply by quadrant number, to ensure total ordering
        //yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players operation @e[tag=newAEC,c=1] XX += @e[tag=FWD,r=0,c=1] YY"
        //yield "execute @e[tag=newAEC] ~ ~ ~ scoreboard players operation @e[tag=newAEC,c=1] XX -= @e[tag=BWD,r=0,c=1] YY"
        yield "entitydata @e[tag=newAEC] {Tags:[\"oldAEC\"]}"
        yield "execute @e[tag=allrun,c=1] ~ ~ ~ scoreboard players set G XX 0"  // constantly set to 0 while still running
        yield sprintf "scoreboard players test G XX 1 *"  // check if done summoning all AECs, if so...
        yield sprintf "C scoreboard players remove @e[tag=oldAEC] XX 1"  // decrement everyone
        yield sprintf "C execute @e[tag=oldAEC,score_XX_min=0,score_XX=0] ~ ~ ~ setblock ~ ~ ~ stone" // look for least guy, set block, and if that succeeds (was a least guy, unique block)
        yield sprintf "C tp @e[tag=oldAEC,score_XX_min=1] ~ ~%d ~" DY  // tp everyone else
        yield sprintf "scoreboard players add G XX 1"  // be able to detect when done using this
*)
#endif
        // Y++
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
        // test if DD > 0
        yield "scoreboard players set @e Temp 0"
        yield "scoreboard players set @e[tag=allrun,score_DD_min=1] Temp 1"
        yield "tp @e[tag=SNX,score_Temp_min=1] ~-1 ~ ~"
        yield "tp @e[tag=SPZ,score_Temp_min=1] ~ ~ ~1"
        yield "tp @e[tag=SPX,score_Temp_min=1] ~1 ~ ~"
        yield "tp @e[tag=SNZ,score_Temp_min=1] ~ ~ ~-1"
        yield "execute @e[tag=allrun,score_Temp_min=1] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] DD -= @e[tag=allrun,c=1] XX"
        yield "execute @e[tag=allrun,score_Temp_min=1] ~ ~ ~ scoreboard players operation @e[tag=allrun,c=1] DD -= @e[tag=allrun,c=1] XX"
    |]

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

let preGenWorldCommonInit =
    [|
        "gamerule commandBlockOutput false"
        "setworldspawn 0 80 0"
        "gamemode 3 @a"  // spec to not spawn hostile mobs
        "scoreboard objectives add Dir dummy"
        "scoreboard objectives add Iter dummy"
        "scoreboard objectives add Remain dummy"  // num times remaining to TP this way
        "scoreboard objectives add Info dummy"
        "scoreboard objectives add Running dummy"
        "scoreboard players set Ticks Info 0"
        "scoreboard players set RadiusCompletedSoFar Info 0"
        "scoreboard players set @a Running -1"
        "scoreboard players set @a Dir 1"
        "scoreboard players set @a Iter 1"
        "scoreboard players set @a Remain 1"
        "scoreboard objectives setdisplay sidebar Info"
        // "give @a filled_map 1 0" can't change scale, can't use in spectator mode, can't change center
        """tellraw @a [{"text":"AFK World Generator for Minecraft 1.9","color":"green"}]"""
        """tellraw @a [{"text":"by Dr. Brian Lorgon111","color":"yellow"}]"""
        """tellraw @a [{"text":"https://www.youtube.com/user/lorgon111","clickEvent":{"action":"open_url","value":"https://www.youtube.com/user/lorgon111"}}]"""
        """tellraw @a [{"text":"This one-command contraption only works when placed near 0,0 (which is now the world spawn).","color":"red"}]"""
        """tellraw @a [{"text":"Set your render distance to 8 chunks!","color":"red"}]"""
        """tellraw @a [{"text":"To start, run this command:"}]"""
        """tellraw @a [{"text":"/scoreboard players set @a Running 1","color":"green"}]"""
        """tellraw @a [{"text":"Once desired size reached, stop with:"}]"""
        """tellraw @a [{"text":"/scoreboard players set @a Running 0","color":"green"}]"""
    |]
let preGenWorld =
    // with D=100, TPT=40, H=130, renderDist=12, looking down, took 22 mins to gen 1000x1000 for me
    // with D=100, TPT=40, H=130, renderDist=8, looking up, spectator mode, took 15 mins to gen 1000x1000 for me
    let D = 100
    let TICKS_PER_TP = 40
    let H = 130
    [|
        "O scoreboard objectives setdisplay sidebar"
        "gamemode 0 @a"
        "gamemode 1 @a"
        "tp @a 0 ~ 0"
        "time set 0"
        "fill ~ ~-33 ~ ~ ~5 ~ air"
        "P scoreboard players test @p Running 1 1" // always running
        "C blockdata ~ ~-2 ~ {auto:1b}"
        sprintf "C tp @a 0 %d 0" H  // if command above succeeded, then they just set Running to 1
        "P scoreboard players test @p Running * 0" // only when 'on'
        "C blockdata ~ ~1 ~ {auto:0b}"
        "C scoreboard players test @p Running 0 0"
        "C blockdata ~ ~12 ~ {auto:1b}"
        // Only run every TICKS_PER_TP
        "scoreboard players add Ticks Info 1"
        sprintf "scoreboard players test Ticks Info %d *" TICKS_PER_TP
        "C scoreboard players set Ticks Info 0"
        "C blockdata ~ ~-2 ~ {auto:1b}"
        "C blockdata ~ ~-1 ~ {auto:0b}"
        "O "
        // what to run
        sprintf "tp @a[score_Dir_min=1,score_Dir=1] ~%d %d ~%d" D H 0
        sprintf "tp @a[score_Dir_min=2,score_Dir=2] ~%d %d ~%d" 0 H D
        sprintf "tp @a[score_Dir_min=3,score_Dir=3] ~%d %d ~%d" -D H 0
        sprintf "tp @a[score_Dir_min=4,score_Dir=4] ~%d %d ~%d" 0 H -D
        "scoreboard players remove @a Remain 1"
        "scoreboard players test @p Remain 0 0"
        "C execute @p[score_Dir_min=2,score_Dir=2] ~ ~ ~ scoreboard players add @a Iter 1"
        "scoreboard players test @p Remain 0 0"
        "C execute @p[score_Dir_min=4,score_Dir=4] ~ ~ ~ scoreboard players add @a Iter 1"
        "scoreboard players test @p Remain 0 0"
        "C scoreboard players operation @a Remain = @p Iter"
        "C scoreboard players add @a Dir 1"
        "C scoreboard players set @a[score_Dir_min=5,score_Dir=5] Dir 1"
        sprintf "C scoreboard players add RadiusCompletedSoFar Info %d" D
    |]

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

let preGenTestWithEDinit =
    [|
        yield "gamerule commandBlockOutput false"
        yield "summon Pig ~-64 250 ~ {NoAI:1,Tags:[\"A\"]}"
    |]
let preGenTestWithED =
    [|
        yield "P tp @e[tag=A] ~128 ~ ~" 
        yield "C say go pig" 
        for cx = 0 to 8 do
            for cz = -8 to 7 do
                yield sprintf "execute @e[tag=A] ~%d 1 ~%d detect ~ ~ ~ air 0 say a" (cx*16) (cz*16)
    |]

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// DEBUG STRATEGIES
// printing with tellraw and ticks
// slowing down loop by having 'P' conditioned on modulo ticks
// using armor stands rather than AECs to see

let floodfillCommonInit = 
    [|
        "fill ~ ~1 ~ ~ ~180 ~ air"
        "gamerule commandBlockOutput false"
        "scoreboard objectives add XP dummy"
        "scoreboard objectives add YP dummy"
        "scoreboard objectives add ZP dummy"
        "scoreboard objectives add XN dummy"
        "scoreboard objectives add YN dummy"
        "scoreboard objectives add ZN dummy"
        "scoreboard objectives add Count dummy"
        "scoreboard objectives add Running dummy"
        "scoreboard players set @a Running 0"
        """give @p minecraft:spawn_egg 1 0 {EntityTag:{id:Bat,NoAI:1,Silent:1,Tags:["StartFloodfill"]},display:{Name:"StartFloodfill"}}"""
        """give @p minecraft:spawn_egg 1 0 {EntityTag:{id:Wolf,NoAI:1,Silent:1,Tags:["ChooseFillBlock"]},display:{Name:"ChooseFillBlock"}}"""
        """tellraw @a [{"text":"Floodfill-3D by Dr. Brian Lorgon111","color":"yellow"}]"""
        """tellraw @a ["You must replace REPLACE (3x) with coordinates of orange command block!"]"""
        """tellraw @a ["Use 'StartFloodfill' on top of a block you want to floodfill-replace"]"""
        """tellraw @a ["Later use 'ChooseFillBlock' on top of the kind of block you want to fill the region with"]"""
    |]
let floodfill = 
    [|
//        let TYPE = "ArmorStand"
        let TYPE = "AreaEffectCloud"
        yield "P "
        // find next
        for s,x,y,z in ["XP",1,0,0;"YP",0,1,0;"ZP",0,0,1;"XN",-1,0,0;"YN",0,-1,0;"ZN",0,0,-1] do
            yield sprintf "stats entity @e[tag=TT] set SuccessCount @e[tag=TT,c=1] %s" s
            yield sprintf "scoreboard players set @e[tag=TT] %s -1" s
            yield sprintf "execute @e[tag=TT] ~ ~ ~ testforblocks ~ ~ ~ ~ ~ ~ ~%d ~%d ~%d" x y z
        yield "stats entity @e[tag=TT] clear SuccessCount"
        for s,x,y,z in ["XP",1,0,0;"YP",0,1,0;"ZP",0,0,1;"XN",-1,0,0;"YN",0,-1,0;"ZN",0,0,-1] do
            yield sprintf "execute @e[tag=TT,score_%s_min=1] ~ ~ ~ summon %s ~%d ~%d ~%d {NoGravity:1,Duration:999999,Tags:[\"TN\"]}" s TYPE x y z
        yield sprintf """execute @e[tag=TT] ~ ~ ~ setblock ~ ~ ~ command_block"""
        yield sprintf """execute @e[tag=TT] ~ ~ ~ summon %s ~ ~ ~ {NoGravity:1,Duration:999999,Tags:["Old"]}""" TYPE
        // prep next iter
        yield "kill @e[tag=TT]"
        yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
        yield """C execute @p[score_Running_min=1] ~ ~ ~ tellraw @a ["...done! Use 'ChooseFillBlock' now to select replacement block."]"""
        yield """C scoreboard players set @a Running 0"""
        yield "entitydata @e[tag=TN] {Tags:[\"TT\",\"kill\"]}"
        yield "execute @e[tag=TT] ~ ~ ~ scoreboard players tag @e[tag=TT,r=0,c=-1] remove kill"
        yield "kill @e[tag=kill]"
        yield "scoreboard players set TT Count 0"
        yield "execute @e[tag=TT] ~ ~ ~ scoreboard players add TT Count 1"
        yield "scoreboard players set Old Count 0"
        yield "execute @e[tag=Old] ~ ~ ~ scoreboard players add Old Count 1"
        // throttle
        yield "scoreboard players test Old Count 1000 *"
        yield """C execute @p[score_Running_min=1] ~ ~ ~ tellraw @a ["Maximum limit of 1000 blocks to change reached"]"""
        yield """C execute @p[score_Running_min=1] ~ ~ ~ tellraw @a ["Use 'ChooseFillBlock' now to select replacement block."]"""
        yield "C scoreboard players set @a Running 0"
        yield "C kill @e[tag=TT]"
        // start floodfill
        yield sprintf "execute @e[tag=StartFloodfill] ~ ~-1 ~ summon %s ~ ~ ~ {NoGravity:1,Duration:999999,Tags:[\"TT\"]}" TYPE
        yield """execute @e[tag=StartFloodfill] ~ ~ ~ tellraw @a ["Starting floodfill..."]"""
        yield "execute @e[tag=StartFloodfill] ~ ~ ~ scoreboard players set @a Running 1"
        yield "kill @e[tag=StartFloodfill]"
        // choose block
        yield "execute @e[tag=ChooseFillBlock] ~ ~ ~ scoreboard players set @a Running 0"
        yield "execute @e[tag=ChooseFillBlock] ~ ~-1 ~ clone ~ ~ ~ ~ ~ ~ REPLACE"
        yield "execute @e[tag=ChooseFillBlock] ~ ~ ~ execute @e[tag=Old] ~ ~ ~ clone REPLACE REPLACE ~ ~ ~"
        yield "execute @e[tag=ChooseFillBlock] ~ ~ ~ kill @e[tag=Old]"
        yield "kill @e[tag=ChooseFillBlock]"
        yield "O "  // coords of blocks to REPLACE
        yield ""  
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
    let s = makeOneCommandBlockWith(drawSpiralCommonInit,[|drawSpiral|])
    let s = makeOneCommandBlockWith(preGenWorldCommonInit,[|preGenWorld|])
    //let s = makeOneCommandBlockWith(preGenTestWithEDinit,[|preGenTestWithED|])
    //let s = makeOneCommandBlockWith(floodfillCommonInit,[|floodfill|])
    //let s = makeOneCommandBlock(OldContraptions.drawCircle)
    System.Windows.Clipboard.SetText(s)
    printfn "%s" (s)
    printfn ""
    printfn "left  %d" (s |> Seq.filter (fun c -> c='{') |> Seq.length)
    printfn "right %d" (s |> Seq.filter (fun c -> c='}') |> Seq.length)
    printfn "chars %d" (s.Length)
    //printfn "blocks %d" (cmds.Length)
    printfn ""
    printfn "it's now in your clipboard"



