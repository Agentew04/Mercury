.globl prompt_name

.macro print_str buffer
	la $a0, \buffer
	li $v0, 4
	syscall
.endmacro

.macro read_str buffer, size
	la $a0, \buffer
	li $a1, \size
	li $v0, 8
	syscall
.endmacro 

.macro read_int destination
	li $v0, 5
	syscall
	add \destination, $zero, $v0
.endmacro

.macro print_int value_register
	add $a0, $zero, \value_register
	li $v0, 1
	syscall
.endmacro

prompt_name:
	print_str p1
	# ask about name
	read_str buffer, 64
	print_str p2
	print_str buffer
	
	# ask about age
	print_str p3
	read_int $t0

	# -years until 18 = AGE - 18
	sub $t1, $t0, 18
	bltz $t1, minor
		# age >= 18
		print_str p4.1
	j endif
	minor:
		# age < 18
		print_str p4.2
		sub $t1, $zero, $t1 # +years = 0 - (-years)
		print_int $t1
		print_str p5
	endif:
	jr $ra
	
	
.data
p1: .asciiz "Hello! What's your name? "
p2: .asciiz "Nice to meet you "
p3: .asciiz "! \nHow old are you? "
p4.1: .asciiz "Awesome, you already can drive a car!"
p4.2: .asciiz "You need to wait about "
p5: .asciiz " years before you can drive :("
buffer: .space 64

.space 512
a: .word 1337