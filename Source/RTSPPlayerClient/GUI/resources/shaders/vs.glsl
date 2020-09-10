attribute 	highp 		vec4 	vertex_coord_in		;
attribute 	mediump 	vec4 	texture_coord_in	;
varying 	mediump 	vec4 	texture_coord_out	;
uniform 	mediump 	mat4 	matrix				;

void main(void) {
	gl_Position 		= matrix * vertex_coord_in	;
	texture_coord_out 	= texture_coord_in			;
}

