% xl -nobuiltins -parse example.xl -show
0! is 1
N! is N * (N - 1)!
for I in 1 .. 5 loop
    print "Factorial of ",I, " is ",I!
