
CREATE FIVEWORDS 28785 ALLOT
FIVEWORDS CONSTANT FIVEADDR ( addr - 15457)

CREATE GUESS 5 ALLOT
GUESS CONSTANT GUESSADDR ( addr - - 20038)

: PRINTFIVE
  ( addr --)
  DUP 5 + SWAP
  DO
    I C@ EMIT
  LOOP
;

: WORDADDR
  ( idx -- addr)
  5 * FIVEADDR + ( add idx to base addr)
;

0 VARIABLE SEED

: SEEDON
  ( ― next value of SEED)
  SEED @ 75 U* 75 0 D+
  OVER OVER U< - -
  1- DUP SEED !
;

: RND
  ( n ―pseudo random no. between 0 and n—1)
  SEEDON U* SWAP DROP
;

: RAND
  ( value for seed ― )
  ?DUP 0=
  IF
  15403 @ SWAP
  THEN
  SEED !
;

( in nested loops, i refers to inner most loop, j refers to outer loop)

: SCORE
  ( addr_ans, addr2_guess -- result)

  1 ( -- Track a complete match)

  5 0 ( iterate over guess)
  DO
    3 PICK 3 PICK ( duplicate top two items in order)
    I + C@ SWAP
    I + C@
    =
    IF
      . " M"
      1 AND ( and result with 1)
    ELSE

      ( Not a direct match, so iterate over ans and find partial match)

      0           ( -- Track P state)
      4 PICK DUP 5 + SWAP ( iterate over answer)
      DO
        3 PICK J + C@ ( -- get guess)
        I C@ ( -- get ans)
        = OR ( -- or result)
      LOOP

      0=
      IF
        . " X"
      ELSE
        . " P"
      THEN

      DROP 0 ( replace result with failure)
    THEN
  LOOP

  ( We need to drop the two addr, leave result state on stack)
  ROT ROT DROP DROP
  ;

  : INPUT
  ( guess index - guess addr)
    0
    BEGIN
      INKEY DUP
      0 = ( no value entered)
      IF
        DROP ( drop from stack)
      ELSE
        DUP 5 = ( pressed the delete key)
        IF
          DROP ( drop the inkey)
          DUP 0 = ( check if this is the first keypress)
          IF
          ELSE
            1- ( decrement step counter)
            ( -- we should be able to pick this value off)
            6 3 PICK 2 * + 9 4 PICK + AT ( Set char pos)
            ."  " ( overwrite)
          THEN
        ELSE
          DUP DUP 64 > SWAP 91 < AND ( -- shift upper key press)
          IF
            32 +
          THEN
          DUP DUP 96 > SWAP 123 < AND ( -- only take lower key presses)
          IF
            DUP GUESSADDR 4 PICK + C! ( write to guess)
            ( we're going to need to update our output position)
            6 4 PICK 2 * + 9 4 PICK + AT ( Set char pos)
            EMIT ( Emit char)
            1+ ( increment step counter)
          THEN
        THEN
      1500 0
      DO LOOP ( per key debounce)
    THEN
    0 0 AT DUP .
    DUP 4 > ( -- check if we've hit the key press limit)
    UNTIL
    DROP ( drop idx)
    DROP ( drop row)
    GUESSADDR (-- EMIT THE GUESS ADDRESS)
  ;

  : ASSESS
  ( -- Assess how the player did, based on result )
    DUP 5 > IF ." unbelievable! " ELSE
    DUP 4 > IF ." outstanding! " ELSE
    DUP 3 > IF ." impressive. " ELSE
    DUP 2 > IF ." pretty good. " ELSE
    DUP 1 > IF ." that'll do. " ELSE
    DUP 0 > IF ." phew. " ELSE
    ." better luck next time. "
    THEN THEN THEN THEN THEN THEN DROP
  ;

  : COMPARE
  ( idx -- result)
  ( compare input address against location and determine if value is found, higher or lower)
    WORDADDR
    5 0
    DO
      DUP I + C@
      GUESSADDR I + C@
      SWAP
      - DUP 0= 0=
      IF
        SWAP DROP ( leave diff on stack, and exit)
        LEAVE
      ELSE
        DROP
        I 4 = IF
          0 SWAP DROP
        THEN
      THEN
    LOOP
  ;

  : BSEARCH
  ( L, R -- Idx)
  ( search result array for guess value)

    ( first element on stack will be L)
    ( second element on stack will be R)

    ( if left hand is greater than right hand, then not found)
    OVER OVER <
    IF
      DROP DROP ( reset stack)
      -1
    ELSE
      OVER OVER + 2 / DUP COMPARE
      DUP 0=
      IF
        ( -- we have a match, drop everything bar the found index)
        DROP ROT ROT DROP DROP
      ELSE
        ( -- test for negative, positive)
        0<
        IF
          1- ROT DROP SWAP BSEARCH ( L, R = m-1)
        ELSE
          1+ SWAP DROP BSEARCH ( L = m+1, R)
        THEN
      THEN
    THEN
  ;

  : GAME
  ( -- Simple first pass game loop)

    ( Setup the answer word)
    CLS 5757 RND WORDADDR

    0 0 AT

    CR ." Guess the JURDLE, in six tries."
    CR ." Each guess must be a valid five-letter word."
    CR ." Press ENTER to submit."

    0 ( game state)

    6 0 ( loop)
    DO
      6 I 2 * + 0 AT ( Set first line pos)
      ." Guess " i 1+ . ." : "
      OVER I input ."  "

      ( check for validity before score)
      5757 0 BSEARCH -1 =
      if
        ." not in word list"
        DROP DROP ( reset stack for input)
        0 ( no loop progression)
        6 I 2 * + 9 AT ( Blank prior guess)
        6 SPACES
        \ 6 I 2 * + 0 AT ( Reset the guess position)
      else
        17 SPACES ( clear any previous message)
        6 I 2 * + 15 AT ( Set answer position)
        SCORE ( exit state of compare is win condition)
        IF
          DROP ( -- remove previous game state)
          6 I - ( Get score)
          LEAVE
        THEN
        1 ( loop progression)
      then
    +LOOP

    DUP
    CR CR ASSESS ( print answer)

    0=
    IF
      CR CR ." The answer is... "
      5 TYPE
    ELSE
      DROP
    THEN
  ;

5757 RND WORDADDR PRINTFIVE ( print a random five letter word)
