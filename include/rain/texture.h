#ifndef RAIN__TEXTURE_H_
#define RAIN__TEXTURE_H_
#include <sokol_gfx.h>
#include <rain/compat.h>
#include <rain/math.h>

/** @see stb_image */
enum rain_texture_format {
	RAIN_TEXTURE_FORMAT_UNKNOWN = 0,
	RAIN_TEXTURE_FORMAT_GREY = 1,
	RAIN_TEXTURE_FORMAT_GREY_ALPHA = 2,
	RAIN_TEXTURE_FORMAT_RGB = 3,
	RAIN_TEXTURE_FORMAT_RGB_ALPHA = 4,
};

struct rain_texture {
	bool exists;
	sg_image image;
	int width, height;
	sg_pixel_format format;
	sg_usage usage;
};

void rain_texture_from_file(
	struct rain_texture *RAIN_RESTRICT this_,
	const char *RAIN_RESTRICT path,
	enum rain_texture_format format,
	sg_usage usage
);

void rain_texture_destroy(struct rain_texture *this_);

#endif // RAIN__TEXTURE_H_
