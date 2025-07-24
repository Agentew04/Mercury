.rodata
.text
.data
.bss
.org 5
.section aa
.pushsection adad
.popsection
.hidden __filestart
.text
__filestart:
.global add2
.align 2
# recebe a0 e a1 e soma
# retorna no v0
add2:
    .L.2.8: add $v0, $a0, $a1
    .L.2.9: jr $ra
    .L.2.10: nop

label1:label2:
label3: label4: # comment
label5: # comment
# comment
label6: label7: add $t0, $t0, $t0
label8: add $t0, $t0, $t0
add $t0, $t0, $t0
label9: label10: add $t0, $t0, $t0 # comment
label11: add $t0, $t0, $t0 # comment
add $t0, $t0, $t0 # comment
.section metadata, "", @progbits
.quad filestart
.asciiz "a.s"
.word 1