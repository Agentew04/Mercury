.hidden __filestart
__filestart:
.global __start
__start:
    # so da exit
    L.1.6: addi $a0, $zero, 5
    L.1.7: addi $a1, $zero, 9
    L.1.8: jal add2
    L.1.9: add $t0, $zero, $v0

    L.10: j __end
.section metadata, "", @progbits
.quad filestart
.asciiz "helloworld.s"
.word 0