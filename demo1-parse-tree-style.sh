% xl -nobuiltins -parse example.xl -style debug -show
(infix CR
 (infixis
  (postfix
   0
   !)
  1
 )
 (infix CR
  (infixis
   (postfix
    N
    !)
   (infix*
    N
    (postfix
     (block( )
      (infix-
       N
       1))
     !)

   ))
  (infixloop
   (prefix
    for
    (infixin
     I
     (infix..
      1
      5)))
   (block indent
    (prefix
     print
     (infix,
      "Factorial of "
      (infix,
       I
       (infix,
        " is "
        (postfix
         I
         !)
       ))))))))
