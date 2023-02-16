// #version 330 core
#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec3 squares_position;
// Values that stay constant for the whole mesh.
uniform mat4 MVP;

void main(){

  vec3 new_modelspace = vertexPosition_modelspace+squares_position;

	// Output position of the vertex, in clip space : MVP * position
	gl_Position =  MVP * vec4(new_modelspace ,1);

}


// // #version 330 core
// #version 330 core

// // Input vertex data, different for all executions of this shader.
// layout(location = 0) in vec3 vertexPosition_modelspace;
// layout(location = 1) in vec3 squares_position;
// // Values that stay constant for the whole mesh.
// uniform mat4 MVP;

// void main(){

//   vec3 new_modelspace = vertexPosition_modelspace+squares_position;

// 	// Output position of the vertex, in clip space : MVP * position
// 	gl_Position =  MVP * vec4(new_modelspace ,1);

// }