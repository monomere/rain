#include <rain/window.h>
#include <stdio.h>
#include <stdlib.h>
#include <GL/gl3w.h>
#include <sokol_gfx.h>
#include <string.h>
#include "glfw.h"

struct rain__window_handle_ {
	GLFWwindow *w;
	char *title;
};

static void rain__glfw_error_(int error, const char *message) {
	fprintf(stderr, "glfw/ERROR(%d) %s\n", error, message);
}

void rain_window_init(
	struct rain_window *restrict this,
	const char *restrict title,
	int width, int height
) {
	this->handle = calloc(1, sizeof(struct rain__window_handle_));
	this->handle->title = strdup(title);

	glfwSetErrorCallback(&rain__glfw_error_);
	if (!glfwInit()) exit(1);

	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GLFW_TRUE);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	this->handle->w = glfwCreateWindow(width, height, this->handle->title, nullptr, nullptr);
	glfwMakeContextCurrent(this->handle->w);
	if (gl3wInit() < 0) {
		fprintf(stderr, "gl3w/ERR failed to initialize :(\n");
		exit(1);
	}
	glfwSwapInterval(1);
}

void rain_window_frame(struct rain_window *this) {
	glfwSwapBuffers(this->handle->w);
	glfwPollEvents();
}

bool rain_window_should_close(struct rain_window *this) {
	return glfwWindowShouldClose(this->handle->w);
}

struct sg_context_desc rain_window_get_context(struct rain_window *this) {
	return (struct sg_context_desc){};
}

void rain_window_get_fb_size(
	struct rain_window *restrict this,
	int *restrict width, int *restrict height
) {
	glfwGetFramebufferSize(this->handle->w, width, height);
}

void rain_window_deinit(struct rain_window *this) {
	glfwDestroyWindow(this->handle->w);
	glfwTerminate();
	free(this->handle->title);
	free(this->handle);
}

float rain_window_get_time(struct rain_window *this) {
	return glfwGetTime();
}

bool rain_window_is_key_down(struct rain_window *this, int keycode) {
	return glfwGetKey(this->handle->w, keycode);
}

void rain_window_set_title(
	struct rain_window *restrict this,
	const char *restrict title
) {
	glfwSetWindowTitle(this->handle->w, title);
	free(this->handle->title);
	this->handle->title = strdup(title);
}

const char *rain_window_get_title(const struct rain_window *this) {
	return this->handle->title;
}
