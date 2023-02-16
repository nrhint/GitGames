//Nathan Hinton

//http://www.opengl-tutorial.org/beginners-tutorials/tutorial-1-opening-a-window/
//https://www.glfw.org/docs/latest/quick.html

#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <GL/freeglut_std.h>

#include <iostream>
#include <chrono>
#include <thread>

#include "piece.h"
#include "defaults.h"
#include "loadShaders.hpp"

int main(void){
    //Init variables:
    int level = START_LEVEL;
    int score = 0;
    int frame_num = 0;
    int last_frame_time = 0;
    int move_piece_direction = 0;
    int ch;
    Piece p;
    std::vector<std::vector<int>> landed_map (GAME_HEIGHT, std::vector<int> (GAME_WIDTH, 0));
    std::vector<std::vector<int>> full_map (GAME_HEIGHT, std::vector<int> (GAME_WIDTH, 0));
    std::vector<Piece> pieces;
    bool run = true;
    bool active_piece = false;
    bool fast_down = false;

    //Initialize GLFW
    if (!glfwInit()){
        std::cout << "FAILED TO INITIALIZE GLFW";
        throw std::runtime_error("error starting glfw");
    }
    //Create the window:
    glfwWindowHint(GLFW_SAMPLES, 4); // 4x antialiased
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3); //Set the version to 3.3
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

    //Open a window and create the context:
    GLFWwindow* window;
    window = glfwCreateWindow(GAME_WIDTH*PIECE_SIZE, GAME_HEIGHT*PIECE_SIZE, "Tetris", NULL, NULL);
    if (NULL == window) {
        std::cout << "Failed to open GLFW window. This can be caused by an intel GPU";
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    
    //Initialize GLEW
    glewExperimental=true; // Needed in core profile
    if (glewInit() != GLEW_OK) {
        fprintf(stderr, "Failed to initialize GLEW\n");
        return -1;
    }
    glfwSetInputMode(window, GLFW_STICKY_KEYS, GL_TRUE);
    glClearColor(0.75f, 0.5f, 0.25f, 0.0f);

    GLuint VertexArrayID;
    glGenVertexArrays(1, &VertexArrayID);
    glBindVertexArray(VertexArrayID);

    //Load shaders:
    GLuint programID = LoadShaders( "vertexShader.vert", "fragmentShader.frag" );

    // Get a handle for our "MVP" uniform
    // Only during the initialisation
    GLuint MatrixID = glGetUniformLocation(programID, "MVP");

    // Projection matrix : 45° Field of View, 4:3 ratio, display range : 0.1 unit <-> 100 units
    glm::mat4 Projection = glm::perspective(glm::radians(45.0f), (float)(GAME_WIDTH*PIECE_SIZE) / (float)(GAME_HEIGHT*PIECE_SIZE), 0.1f, 100.0f);
    // glm::mat4 Projection = glm::perspective(glm::radians(1.0f), 4.0f/3.0f, 0.1f, 100.0f);
    
    // Or, for an ortho camera :
    //glm::mat4 Projection = glm::ortho(-10.0f,10.0f,-10.0f,10.0f,0.0f,100.0f); // In world coordinates
    
    // Camera matrix
    glm::mat4 View = glm::lookAt(
        glm::vec3(0, 0, 1), // Camera is at (4,3,3), in World Space
        glm::vec3(0,0,0), // and looks at the origin
        glm::vec3(0,1,0)  // Head is up (set to 0,-1,0 to look upside-down)
        );
    
    // Model matrix : an identity matrix (model will be at the origin)
    glm::mat4 Model = glm::mat4(1.0f);
    // Our ModelViewProjection : multiplication of our 3 matrices
    glm::mat4 mvp = Projection * View;// * Model; // Remember, matrix multiplication is the other way around

    static GLfloat* g_square_position_data = new GLfloat[GAME_WIDTH * GAME_HEIGHT * 3]; //holds x, y, z (Might be able to get away with just xy but that is a later thing)

    static const GLfloat g_square_vertex_buffer[] = {
        -1.0f/(PIECE_SIZE*4), -1.0f/(PIECE_SIZE*4), 0.0f,
        1.0f/(PIECE_SIZE*4), -1.0f/(PIECE_SIZE*4), 0.0f,
        -1.0f/(PIECE_SIZE*4),  1.0f/(PIECE_SIZE*4), 0.0f,
        1.0f/(PIECE_SIZE*4),  1.0f/(PIECE_SIZE*4), 0.0f,        
    };

    GLuint square_vertex_buffer;
    glGenBuffers(1, &square_vertex_buffer);
    glBindBuffer(GL_ARRAY_BUFFER, square_vertex_buffer);
    glBufferData(GL_ARRAY_BUFFER, sizeof(g_square_vertex_buffer), g_square_vertex_buffer, GL_STATIC_DRAW);

    GLuint squares_position_buffer;
    glGenBuffers(1, &squares_position_buffer);
    glBindBuffer(GL_ARRAY_BUFFER, squares_position_buffer);
    glBufferData(GL_ARRAY_BUFFER, GAME_WIDTH*GAME_HEIGHT * 3 * sizeof(GLfloat), NULL, GL_STREAM_DRAW);


    // static const GLfloat square_matrix[] = {
    //     0-1.0f/(PIECE_SIZE*2), -1.0f/(PIECE_SIZE*2), 0.0f,
    //     1.0f/(PIECE_SIZE*2), -1.0f/(PIECE_SIZE*2), 0.0f,
    //     1.0f/(PIECE_SIZE*2),  1.0f/(PIECE_SIZE*2), 0.0f,
    //     -1.0f/(PIECE_SIZE*2),  -1.0f/(PIECE_SIZE*2), 0.0f,
    //     -1.0f/(PIECE_SIZE*2), 1.0f/(PIECE_SIZE*2), 0.0f,
    //     1.0f/(PIECE_SIZE*2),  1.0f/(PIECE_SIZE*2), 0.0f,
    // };

    // static const GLfloat square_matrix[] = { 
	// 	-1.0f, -1.0f, 0.0f,
	// 	 1.0f, -1.0f, 0.0f,
	// 	 1.0f,  1.0f, 0.0f,
    //      -1.0f, -1.0f, 0.0f,
    //      -1.0f, 1.0f, 0.0f,
    //      1.0f, 1.0f, 0.0f,
	// };

    // // This will identify our vertex buffer
    // GLuint vertexbuffer;
    // // Generate 1 buffer, put the resulting identifier in vertexbuffer
    // glGenBuffers(1, &vertexbuffer);
    // // The following commands will talk about our 'vertexbuffer' buffer
    // glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
    // // Give our vertices to OpenGL.
    // glBufferData(GL_ARRAY_BUFFER, sizeof(square_matrix), square_matrix, GL_STATIC_DRAW);

    while (true == run) {
        //Init for next frame:
        std::chrono::steady_clock::time_point frame_start = std::chrono::steady_clock::now();
        std::cout << "Rendering frame #" << frame_num << std::endl;
        //Get input
        if (GLFW_PRESS == glfwGetKey(window, GLFW_KEY_ESCAPE) && glfwWindowShouldClose(window) == 0) {
            run = false;
        } else if (GLFW_PRESS == glfwGetKey(window, GLFW_KEY_RIGHT)) {
            std::cout << "Right pressed" << std::endl;
            move_piece_direction = 1;
        } else if (GLFW_PRESS == glfwGetKey(window, GLFW_KEY_LEFT)) {
            std::cout << "Left pressed" << std::endl;
            move_piece_direction = -1;
        } else if (GLFW_PRESS == glfwGetKey(window, GLFW_KEY_DOWN)) {
            std::cout << "Down pressed" << std::endl;
            fast_down = true;
        } else {
            move_piece_direction = 0;
            fast_down = false;
        }
        frame_num ++;

        //Game logic:
        full_map = landed_map;
        if (false == active_piece) {
            pieces.push_back(Piece());
            score ++;
            active_piece = true;
        }
        bool piece_moved = false;
        for (auto piece = pieces.begin(); piece != pieces.end(); ++piece) {
            if (false == piece->is_landed()) {
                //Check to make sure that the piece does not collide with anything
                std::vector<positions> piece_positions;
                // if (0 == frame_num%10) {
                //     fast_down = true;
                // }
                std::cout << glutGet(GLUT_ELAPSED_TIME) - last_frame_time;
                if (glutGet(GLUT_ELAPSED_TIME) - last_frame_time > 1000.0f/level) {
                    fast_down = true;
                    last_frame_time = glutGet(GLUT_ELAPSED_TIME);
                }
                piece->test_pos_update(move_piece_direction, fast_down);
                piece_positions = piece->get_draw_positions();
                if (true == fast_down) {
                    for (auto current_position = piece_positions.begin(); current_position != piece_positions.end(); ++current_position) {
                        if (1 == full_map[current_position->y][current_position->x]) {
                            piece->set_landed();
                        }
                    }
                }
                //Either by a collision or by hitting the bottom
                piece_positions = piece->get_draw_positions();
                if (false == piece->is_landed()) {
                    for (auto current_position = piece_positions.begin(); current_position != piece_positions.end(); ++current_position) {
                        full_map[current_position->y][current_position->x] = 1;
                    }
                    piece->finalize_pos_update();
                    piece_moved = true;
                } else {
                    piece->cancel_move();
                }
            } 
            if ( true == piece->is_landed() && false == piece->is_finalized()) {
                std::vector<positions> piece_positions = piece->get_draw_positions();
                for (auto current_position = piece_positions.begin(); current_position != piece_positions.end(); ++current_position) {
                    landed_map[current_position->y][current_position->x] = 1;
                }
                //reset the map with the landed map. This prevents flickers of 0's
                full_map = landed_map;
                piece->set_finalized();
            }
        }
        if (false == piece_moved) {
            active_piece = false;
        }
        //Check for fail condition
        for (int index = 0; index < GAME_WIDTH; index++) {
            if (1 == landed_map[0][index]) {
                std::cout << "GAME OVER" << std::endl << std::endl;
                run = false;
            }
        }
        //Check for lines to be cleared:
        for (auto row = landed_map.begin(); row != landed_map.end(); ++row) {
            bool clear = true;
            for (int index = 0; GAME_WIDTH > index; index ++) {
                if (0 == row->at(index)) {
                    clear = false;
                    break;
                }
            }
            if (true == clear) {
                landed_map.erase(row);
                landed_map.insert(landed_map.begin()+1, std::vector<int> (GAME_WIDTH, 0));
                score += 10;
            }
        }
        //BEGIN OPENGL STUFF:
        int square_count = 0;
        for (int ii = 0; ii < GAME_HEIGHT; ii++) {
            for (int jj = 0; jj < GAME_WIDTH; jj++) {
                if (1 == full_map[ii][jj]) {
                    double x = (double)(jj-((GAME_WIDTH)/2))/(GAME_WIDTH)/2+(0.5/(GAME_WIDTH));
                    double y = (double)-(ii-(GAME_HEIGHT)/2)/(GAME_HEIGHT)+(0.5/GAME_HEIGHT);
                    g_square_position_data[3 * square_count + 0] = x;
                    g_square_position_data[3 * square_count + 1] = y;
                    std::cout << "x: " << x << ", y: " << y <<std::endl;
                    g_square_position_data[3 * square_count + 2] = 0;
                    square_count++;
                }
            }
        }
        //Draw:

        //Clear screen:
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        // glClear(GL_COLOR_BUFFER_BIT);
        
        glBindBuffer(GL_ARRAY_BUFFER, squares_position_buffer);
        glBufferData(GL_ARRAY_BUFFER, GAME_WIDTH * GAME_HEIGHT * 3 * sizeof(GLfloat), NULL, GL_STREAM_DRAW);
        glBufferSubData(GL_ARRAY_BUFFER, 0, GAME_WIDTH * GAME_HEIGHT * sizeof(GLfloat), g_square_position_data);

        glUseProgram(programID);

        // Send our transformation to the currently bound shader, in the "MVP" uniform
        // This is done in the main loop since each model will have a different MVP matrix (At least for the M part)
        glUniformMatrix4fv(MatrixID, 1, GL_FALSE, &mvp[0][0]);

        // 1rst attribute buffer : vertices
		glEnableVertexAttribArray(0);
		glBindBuffer(GL_ARRAY_BUFFER, square_vertex_buffer);
		glVertexAttribPointer(
			0,                  // attribute. No particular reason for 0, but must match the layout in the shader.
			3,                  // size
			GL_FLOAT,           // type
			GL_FALSE,           // normalized?
			0,                  // stride
			(void*)0            // array buffer offset
		);


        // 2nd attribute buffer : positions
        glEnableVertexAttribArray(1);
        glBindBuffer(GL_ARRAY_BUFFER, squares_position_buffer);
        glVertexAttribPointer(
            1,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
            3,                  // size = x, y, z
            GL_FLOAT,           // type
            GL_FALSE,           // normalized?
            0,                  // stride
            (void*)0            // array buffer offset
        );
        // Draw the triangle !
        // for (int index = 0; index < square_count; index ++) {
        //     glDrawArrays(GL_TRIANGLES, 0, 3);
        // }
        glVertexAttribDivisor(0, 0);
        glVertexAttribDivisor(1, 1);

        glDrawArraysInstanced(GL_TRIANGLE_STRIP, 0, 4, square_count); // Starting from vertex 0; 3 vertices total -> 1 triangle
        glDisableVertexAttribArray(0);
        glDisableVertexAttribArray(1);

        //Swap buffers:
        glfwSwapBuffers(window);
        glfwPollEvents();


        if (true == true) {
            // for (int xx = 0; xx < 30; xx ++) {
            //     std::cout << std::endl;
            // }
            std::cout << "Score: " << score << std::endl;
            for (int row = 0; row < GAME_HEIGHT; row ++) {
                for (int index = 0; index < GAME_WIDTH; index ++) {
                    if (0 == full_map[row][index]){
                        std::cout << full_map[row][index] << " ";
                    } else {
                        std::cout << "\033[7m1 \033[0m";
                    }
                }
                std::cout << std::endl;
            }
        }

        // //TODO: change to instead of sleeping just track time earlier and loop at full speed and check if one second passed
        // std::chrono::nanoseconds nanoseconds_to_sleep ((1000000000/level) - (std::chrono::steady_clock::now()-frame_start).count());
        // // std::cout << nanoseconds_to_sleep.count() << std::endl;
        // std::this_thread::sleep_for(std::chrono::nanoseconds(nanoseconds_to_sleep));
    }
    // Cleanup VBO and shader
	glDeleteBuffers(1, &squares_position_buffer);
	glDeleteProgram(programID);
	glDeleteVertexArrays(1, &VertexArrayID);

	// Close OpenGL window and terminate GLFW
	glfwTerminate();

    return 0;
}