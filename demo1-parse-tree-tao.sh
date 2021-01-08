% xl -nobuiltins -parse example.xl -style ~/Work/xl/src/tao -show
// Automatically generated, do not modify
tao_infix "CR",
  tao_left
    tao_infix "is",
      tao_left
        tao_postfix
          tao_left
            tao_integer 0
          tao_right
            tao_name "!"
      tao_right
        tao_integer 1

  tao_right
    tao_infix "CR",
      tao_left
        tao_infix "is",
          tao_left
            tao_postfix
              tao_left
                tao_name "N"
              tao_right
                tao_name "!"
          tao_right
            tao_infix "*",
              tao_left
                tao_name "N"
              tao_right
                tao_postfix
                  tao_left
                    tao_block "( )",
                      tao_infix "-",
                        tao_left
                          tao_name "N"
                        tao_right
                          tao_integer 1
                  tao_right
                    tao_name "!"

      tao_right
        tao_infix "loop",
          tao_left
            tao_prefix
              tao_left
                tao_name "for"
              tao_right
                tao_infix "in",
                  tao_left
                    tao_name "I"
                  tao_right
                    tao_infix "..",
                      tao_left
                        tao_integer 1
                      tao_right
                        tao_integer 5
          tao_right
            tao_block "Indent",
              tao_prefix
                tao_left
                  tao_name "print"
                tao_right
                  tao_infix ",",
                    tao_left
                      tao_text "Factorial of "
                    tao_right
                      tao_infix ",",
                        tao_left
                          tao_name "I"
                        tao_right
                          tao_infix ",",
                            tao_left
                              tao_text " is "
                            tao_right
                              tao_postfix
                                tao_left
                                  tao_name "I"
                                tao_right
                                  tao_name "!"

    // End of automatically generated code
