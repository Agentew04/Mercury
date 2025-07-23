__filestart:
.global add2
.align 2
# recebe a0 e a1 e soma
# retorna no v0
add2:
    add $v0, $a0, $a1
    jr $ra
    nop

.section metadata, "", @progbits
.quad filestart
.asciiz "a.s"