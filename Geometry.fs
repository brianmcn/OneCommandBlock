module Geometry

let commonInit =
    [|
        "gamerule commandBlockOutput false"
        "scoreboard objectives add lineDraw dummy"
        "scoreboard objectives add LDZrev dummy"
        "scoreboard objectives add LDYrev dummy"
        "scoreboard objectives add LDXrev dummy"
        "scoreboard objectives add endpoint dummy"
        "tp @p ~ ~110 ~"
    |]

let cmdsDrawLine = // assuming DX DY and DZ are set in lineDraw objective, then this draws the line from LDstart to relative (DX,DY,DZ) once LDstart exists
    [|
        // deal with negative DX
        yield "P scoreboard players set @e[tag=LDstart] LDXrev 0"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players test DX lineDraw * -1"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players set Temp lineDraw 0"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation Temp lineDraw -= DX lineDraw"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation DX lineDraw = Temp lineDraw"
        yield "C scoreboard players set @e[tag=LDstart] LDXrev 1"
        // deal with negative DY
        yield "scoreboard players set @e[tag=LDstart] LDYrev 0"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players test DY lineDraw * -1"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players set Temp lineDraw 0"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation Temp lineDraw -= DY lineDraw"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation DY lineDraw = Temp lineDraw"
        yield "C scoreboard players set @e[tag=LDstart] LDYrev 1"
        // deal with negative DZ
        yield "scoreboard players set @e[tag=LDstart] LDZrev 0"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players test DZ lineDraw * -1"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players set Temp lineDraw 0"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation Temp lineDraw -= DZ lineDraw"
        yield "C execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation DZ lineDraw = Temp lineDraw"
        yield "C scoreboard players set @e[tag=LDstart] LDZrev 1"
        // init vars, choose major axis
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TempZ lineDraw = DX lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TempZ lineDraw -= DZ lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players set @e[tag=LDstart] lineDraw 0"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation MAJOR lineDraw = DZ lineDraw"  
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players test TempZ lineDraw 0 *"
        yield "C scoreboard players set @e[tag=LDstart] lineDraw 1"
        yield "C scoreboard players operation MAJOR lineDraw = DX lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TempY lineDraw = DY lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TempY lineDraw -= MAJOR lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players test TempY lineDraw 0 *"
        yield "C scoreboard players set @e[tag=LDstart] lineDraw 2"
        yield "C scoreboard players operation MAJOR lineDraw = DY lineDraw"
        // setup '2dx/2dy/2dz' vars
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TDZ lineDraw = DZ lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TDZ lineDraw += TDZ lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TDX lineDraw = DX lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TDX lineDraw += TDX lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TDY lineDraw = DY lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TDY lineDraw += TDY lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TMAJOR lineDraw = MAJOR lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation TMAJOR lineDraw += TMAJOR lineDraw"
        // A = 2dz - dx
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation AZ lineDraw = TDZ lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation AZ lineDraw -= MAJOR lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation AY lineDraw = TDY lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation AY lineDraw -= MAJOR lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation AX lineDraw = TDX lineDraw"
        yield "execute @e[tag=LDstart] ~ ~ ~ scoreboard players operation AX lineDraw -= MAJOR lineDraw"
        yield "entitydata @e[tag=LDstart] {Tags:[\"LDloop\"]}"
        //yield "execute @e[tag=LDloop] ~ ~ ~ setblock ~ ~ ~ stone"
        yield "execute @e[tag=LDloop] ~ ~ ~ summon AreaEffectCloud ~ ~ ~ {Duration:200,Tags:[\"lineDrawPlace\"]}"
        // loop
        // - diff on X
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AX lineDraw 1 *"
        yield "C tp @e[tag=LDloop,score_LDXrev=0] ~1 ~ ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AX lineDraw 1 *"
        yield "C tp @e[tag=LDloop,score_LDXrev_min=1] ~-1 ~ ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AX lineDraw 1 *"
        yield "C execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AX lineDraw -= TMAJOR lineDraw"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AX lineDraw += TDX lineDraw"
        // - diff on Y
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AY lineDraw 1 *"
        yield "C tp @e[tag=LDloop,score_LDYrev=0] ~ ~1 ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AY lineDraw 1 *"
        yield "C tp @e[tag=LDloop,score_LDYrev_min=1] ~ ~-1 ~"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AY lineDraw 1 *"
        yield "C execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AY lineDraw -= TMAJOR lineDraw"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AY lineDraw += TDY lineDraw"
        // - diff on Z
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AZ lineDraw 1 *"
        yield "C tp @e[tag=LDloop,score_LDZrev=0] ~ ~ ~1"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AZ lineDraw 1 *"
        yield "C tp @e[tag=LDloop,score_LDZrev_min=1] ~ ~ ~-1"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test AZ lineDraw 1 *"
        yield "C execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AZ lineDraw -= TMAJOR lineDraw"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players operation AZ lineDraw += TDZ lineDraw"
        // endloop, stop when done after MAJOR steps
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players remove MAJOR lineDraw 1"
        yield "execute @e[tag=LDloop] ~ ~ ~ scoreboard players test MAJOR lineDraw * -1"
        yield "C kill @e[tag=LDloop]"
    |]

let cmdsComputeDxDyDz = 
    [|
        // computes the dx/dy/dz from A to B (provided it's in within -127...127) 
        let MAX = 128
        let STEPS = [-128;-64;-32;-16;-8;-4;-2;-1]
        yield sprintf "O scoreboard players set DX lineDraw %d" (MAX-1)
        yield sprintf "scoreboard players set DY lineDraw %d" (MAX-0)  // y testing is weird for some reason...
        yield sprintf "scoreboard players set DZ lineDraw %d" (MAX-1)
        yield sprintf "execute @e[tag=LDA] ~ ~ ~ summon ArmorStand ~%d ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDTX\"]}" MAX
        yield sprintf "execute @e[tag=LDA] ~ ~ ~ summon ArmorStand ~ ~%d ~ {Marker:1,NoGravity:1,Tags:[\"LDTY\"]}" MAX
        yield sprintf "execute @e[tag=LDA] ~ ~ ~ summon ArmorStand ~ ~ ~%d {Marker:1,NoGravity:1,Tags:[\"LDTZ\"]}" MAX
        for s in STEPS do
            yield sprintf "execute @e[tag=LDTX] ~%d ~%d ~%d testfor @e[tag=LDB,dx=%d,dy=%d,dz=%d]" (0) (0-MAX) (0-MAX) (s) (2*MAX) (2*MAX)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=LDTX] ~%d ~%d ~%d" s 0 0
            yield sprintf "C scoreboard players remove DX lineDraw %d" (-s)
            
            yield sprintf "execute @e[tag=LDTY] ~%d ~%d ~%d testfor @e[tag=LDB,dx=%d,dy=%d,dz=%d]" (0-MAX) (0) (0-MAX) (2*MAX) (s) (2*MAX)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=LDTY] ~%d ~%d ~%d" 0 s 0
            yield sprintf "C scoreboard players remove DY lineDraw %d" (-s)

            yield sprintf "execute @e[tag=LDTZ] ~%d ~%d ~%d testfor @e[tag=LDB,dx=%d,dy=%d,dz=%d]" (0-MAX) (0-MAX) (0) (2*MAX) (2*MAX) (s)
            yield "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
            yield sprintf "C tp @e[tag=LDTZ] ~%d ~%d ~%d" 0 0 s
            yield sprintf "C scoreboard players remove DZ lineDraw %d" (-s)
        yield "kill @e[tag=LDTX]"
        yield "kill @e[tag=LDTY]"
        yield "kill @e[tag=LDTZ]"
        // call the draw-er
        yield "entitydata @e[tag=LDA] {Tags:[\"LDstart\"]}"
        yield "kill @e[tag=LDB]"
    |]

let kickStart = 
    [|
        // if A placed, and player demands second endpoint, then place it and run the tool
        "P execute @e[tag=LDA] ~ ~ ~ scoreboard players test @p endpoint 1 1"
#if SHULKER
        "C execute @p ~ ~ ~ summon Shulker ~ ~ ~ {NoAI:1}"  // snap to grid
        "C execute @e[type=Shulker] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDB\"]}"
#else
        "C execute @p ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDB\"]}"
#endif
        "C blockdata ~2 ~2 ~ {auto:1b}"  // TODO coords - call out to compute dxdydz
        "C blockdata ~2 ~3 ~ {auto:0b}"
        // if A not yet placed, and player demands an endpoint, place A"
        "testfor @e[tag=LDA]"
        "testforblock ~ ~1 ~ chain_command_block -1 {SuccessCount:0}"
        "C scoreboard players test @p endpoint 1 1"
#if SHULKER
        "C execute @p ~ ~ ~ summon Shulker ~ ~ ~ {NoAI:1}"  // snap to grid
        "C execute @e[type=Shulker] ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDA\"]}"
        "kill @e[type=Shulker]"
#else
        "C execute @p ~ ~ ~ summon ArmorStand ~ ~ ~ {Marker:1,NoGravity:1,Tags:[\"LDA\"]}"
#endif
        // make barrier particles
        "execute @e[tag=lineDrawPlace] ~ ~ ~ particle barrier ~ ~ ~ 0 0 0 0 0 force"
        // if player wants, convert to block he stands on
        "scoreboard players test @p endpoint 2 2"
        """C execute @e[tag=lineDrawPlace] ~ ~ ~ setblock ~ ~ ~ stone"""
        //"""C execute @e[tag=lineDrawPlace] ~ ~ ~ setblock ~ ~ ~ command_block 0 replace {auto:1b,Command:"execute @p ~ ~ ~ ???"}"""  // TODO block type with clone
        "C kill @e[tag=lineDrawPlace]"
        // clear endpoint request
        "scoreboard players set @p endpoint 0"
    |]

let shorten(a:string[]) =
    // in order to fit into one command block, need to shorten some identifiers
    a 
    |> Array.map (fun s -> s.Replace("MAJOR","M"))
    |> Array.map (fun s -> s.Replace("LDstart","J"))
    |> Array.map (fun s -> s.Replace("LDloop","R"))
    |> Array.map (fun s -> s.Replace("LDXrev","XR"))
    |> Array.map (fun s -> s.Replace("LDYrev","YR"))
    |> Array.map (fun s -> s.Replace("LDZrev","ZR"))
    |> Array.map (fun s -> s.Replace("Temp","Tm"))
    |> Array.map (fun s -> s.Replace("LDTX","B"))
    |> Array.map (fun s -> s.Replace("LDTY","C"))
    |> Array.map (fun s -> s.Replace("LDA","F"))
    |> Array.map (fun s -> s.Replace("LDB","G"))
    |> Array.map (fun s -> s.Replace("lineDrawPlace","H"))
    |> Array.map (fun s -> s.Replace("lineDraw","Q"))

// TODO note that can save a lot by changing MAX/STEPS to 64 instead of 128 if needed
// TODO could auto:1b the purples to avoid needing the reds

let All = (shorten commonInit, [| shorten kickStart; shorten cmdsComputeDxDyDz; shorten cmdsDrawLine |])
