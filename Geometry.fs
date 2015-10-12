module Geometry

let commonInit =
    [|
        "R"
        "O gamerule commandBlockOutput false"
        "scoreboard objectives add LD dummy"     // LD is "lineDraw", but need short text to cram into command
        "scoreboard objectives add LDZrev dummy"
        "scoreboard objectives add LDYrev dummy"
        "scoreboard objectives add LDXrev dummy"
        "scoreboard players set @a LD 0"
        "scoreboard objectives add endpoint dummy"  // TODO clean up objectives
    |]

let cmdsDrawLine =
    [|
        // assuming DX DY and DZ are set in LD objective, then this draws the line from LDs to relative (DX,DY,DZ) once LDs exists
        yield "R"
        yield "P"
        // deal with negative DX
        yield "scoreboard players set @e[tag=LDs] LDXrev 0"   // LDs is LDstart, shortened
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players test DX LD * -1"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players set Temp LD 0"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players operation Temp LD -= DX LD"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players operation DX LD = Temp LD"
        yield "C scoreboard players set @e[tag=LDs] LDXrev 1"
        // deal with negative DY
        yield "scoreboard players set @e[tag=LDs] LDYrev 0"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players test DY LD * -1"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players set Temp LD 0"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players operation Temp LD -= DY LD"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players operation DY LD = Temp LD"
        yield "C scoreboard players set @e[tag=LDs] LDYrev 1"
        // deal with negative DZ
        yield "scoreboard players set @e[tag=LDs] LDZrev 0"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players test DZ LD * -1"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players set Temp LD 0"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players operation Temp LD -= DZ LD"
        yield "C execute @e[tag=LDs] ~ ~ ~ scoreboard players operation DZ LD = Temp LD"
        yield "C scoreboard players set @e[tag=LDs] LDZrev 1"
        // init vars, choose major axis
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TempZ LD = DX LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TempZ LD -= DZ LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players set @e[tag=LDs] LD 0"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation M LD = DZ LD"    // M is MAJOR axis length, shortened for command length
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players test TempZ LD 0 *"
        yield "C scoreboard players set @e[tag=LDs] LD 1"
        yield "C scoreboard players operation M LD = DX LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TempY LD = DY LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TempY LD -= M LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players test TempY LD 0 *"
        yield "C scoreboard players set @e[tag=LDs] LD 2"
        yield "C scoreboard players operation M LD = DY LD"
        // setup '2dx/2dy/2dz' vars
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TDZ LD = DZ LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TDZ LD += TDZ LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TDX LD = DX LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TDX LD += TDX LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TDY LD = DY LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TDY LD += TDY LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TM LD = M LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation TM LD += TM LD"
        // A = 2dz - dx
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation AZ LD = TDZ LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation AZ LD -= M LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation AY LD = TDY LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation AY LD -= M LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation AX LD = TDX LD"
        yield "execute @e[tag=LDs] ~ ~ ~ scoreboard players operation AX LD -= M LD"
        yield "entitydata @e[tag=LDs] {Tags:[\"LDloop\"]}"
        yield "execute @e[tag=LDloop] ~ ~ ~ setblock ~ ~ ~ stone"
        // loop
        // - diff on X
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AX LD 1 *"
        yield "C tp @e[tag=LDloop,score_LDXrev=0] ~1 ~ ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AX LD 1 *"
        yield "C tp @e[tag=LDloop,score_LDXrev_min=1] ~-1 ~ ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AX LD 1 *"
        yield "C execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AX LD -= TM LD"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AX LD += TDX LD"
        // - diff on Y
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AY LD 1 *"
        yield "C tp @e[tag=LDloop,score_LDYrev=0] ~ ~1 ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AY LD 1 *"
        yield "C tp @e[tag=LDloop,score_LDYrev_min=1] ~ ~-1 ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AY LD 1 *"
        yield "C execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AY LD -= TM LD"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AY LD += TDY LD"
        // - diff on Z
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AZ LD 1 *"
        yield "C tp @e[tag=LDloop,score_LDZrev=0] ~ ~ ~1"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AZ LD 1 *"
        yield "C tp @e[tag=LDloop,score_LDZrev_min=1] ~ ~ ~-1"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AZ LD 1 *"
        yield "C execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AZ LD -= TM LD"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AZ LD += TDZ LD"
        // endloop, stop when done after M steps
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players remove M LD 1"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test M LD * -1"
        yield "C kill @e[tag=LDloop]"
    |]

let cmdsComputeDxDyDz = 
    [|
        // computes the dx/dy/dz from A to B (provided it's in within -127...127) 
        let MAX = 128
        let STEPS = [-128;-64;-32;-16;-8;-4;-2;-1]
        yield "O "
        yield sprintf "scoreboard players set DX LD %d" (MAX-1)
        yield sprintf "scoreboard players set DY LD %d" (MAX-0)  // y testing is weird for some reason...
        yield sprintf "scoreboard players set DZ LD %d" (MAX-1)
        yield sprintf "execute @e[tag=LDA] ~ ~ ~ summon ArmorStand ~%d ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDTX\"]}" MAX
        yield sprintf "execute @e[tag=LDA] ~ ~ ~ summon ArmorStand ~ ~%d ~ {Marker:1,NoGravity:1,Tags:[\"LDTY\"]}" MAX
        yield sprintf "execute @e[tag=LDA] ~ ~ ~ summon ArmorStand ~ ~ ~%d {Marker:1,NoGravity:1,Tags:[\"LDTZ\"]}" MAX
        for s in STEPS do
            yield sprintf "execute @e[tag=LDTX] ~%d ~%d ~%d testfor @e[tag=LDB,dx=%d,dy=%d,dz=%d]" (0) (0-MAX) (0-MAX) (s) (2*MAX) (2*MAX)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=LDTX] ~%d ~%d ~%d" s 0 0
            yield sprintf "C scoreboard players remove DX LD %d" (-s)
            
            yield sprintf "execute @e[tag=LDTY] ~%d ~%d ~%d testfor @e[tag=LDB,dx=%d,dy=%d,dz=%d]" (0-MAX) (0) (0-MAX) (2*MAX) (s) (2*MAX)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=LDTY] ~%d ~%d ~%d" 0 s 0
            yield sprintf "C scoreboard players remove DY LD %d" (-s)

            yield sprintf "execute @e[tag=LDTZ] ~%d ~%d ~%d testfor @e[tag=LDB,dx=%d,dy=%d,dz=%d]" (0-MAX) (0-MAX) (0) (2*MAX) (2*MAX) (s)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=LDTZ] ~%d ~%d ~%d" 0 0 s
            yield sprintf "C scoreboard players remove DZ LD %d" (-s)
        yield "kill @e[tag=LDTX]"
        yield "kill @e[tag=LDTY]"
        yield "kill @e[tag=LDTZ]"
        // call the draw-er
        yield "entitydata @e[tag=LDA] {Tags:[\"LDs\"]}"
        yield "kill @e[tag=LDB]"
    |]

let kickStart = 
    [|
        "R"
        "P"
        // if A placed, and player demands second endpoint, then place it and run the tool
        "execute @e[tag=LDA] ~ ~ ~ scoreboard players test @p endpoint 1 *"
        "C execute @p ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDB\"]}"
        "C blockdata ~2 ~4 ~ {auto:1b}"  // TODO coords - call out to compute dxdydz
        "C blockdata ~2 ~5 ~ {auto:0b}"
        // if A not yet placed, and player demands an endpoint, place A"
        "testfor @e[tag=LDA]"
        "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
        "C scoreboard players test @p endpoint 1 *"
        "C execute @p ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDA\"]}"
        // clear endpoint request
        "scoreboard players set @p endpoint 0"
        // TODO make barrier particles
    |]

let All = (commonInit, [| kickStart; cmdsComputeDxDyDz; cmdsDrawLine |])
