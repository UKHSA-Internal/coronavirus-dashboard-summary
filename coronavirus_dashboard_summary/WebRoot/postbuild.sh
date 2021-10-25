#!/usr/bin bash

npx postcss ./dist/application.css --use autoprefixer -d ./css
uglifycss ./dist/application.css --output ./dist/application.css
