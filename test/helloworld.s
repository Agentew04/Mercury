.global __start
__start:
    # so da exit
    addi $a0, $zero, 5
    addi $a1, $zero, 9
    jal add2
    add $t0, $zero, $v0