# .data


# .text

# .macro marcos target
# addi \target, $zero, 5
# .endmacro

# marcos $t2
.text
.hidden __filestart
__filestart: # comeca com __, vai ser ignorado pelo aplicativo.
.section metadata, "", @progbits # define secao de metadados que guarda onde no elf esse arquivo comeca
.asciiz "C:/Users/digoa/AppData/Roaming/.saae/resources/templates/basic-io/src/name.asm"
.quad __filestart
.word 3
.text
L.3.1: .globl prompt_name

L.3.3: .macro print_str addr
L.3.4: 	la $a0, \addr
L.3.5: 	li $v0, 4
L.3.6: 	syscall
L.3.7: .endmacro

prompt_name:
L.3.10: 	print_str p1

L.3.12: 	jr $ra
	
	
.data
p1: .asciiz "Hello! What's your name? "
p2: .asciiz
