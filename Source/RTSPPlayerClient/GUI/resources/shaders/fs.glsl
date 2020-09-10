varying 	mediump 	vec4 		texture_coord_out		;
uniform 				sampler2D 	texture					;

void main(void) {
	gl_FragColor = texture2D(texture, texture_coord_out.st)	;
}
