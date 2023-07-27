#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>

void* glfwNativeWindowHandle(GLFWwindow *window);
void glfwDestroyWindowImpl(GLFWwindow *window);
void *getNativeDisplayHandle();
