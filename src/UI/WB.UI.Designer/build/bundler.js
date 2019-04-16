const join = require("path").join;
const { src, dest, series } = require("gulp");
const cache = require("gulp-cache");
const concat = require("gulp-concat");
const filter = require("gulp-filter");
const gulpif = require("gulp-if");
const less = require("gulp-less");
const merge = require("merge2");
const rev = require("gulp-rev");
const terser = require("gulp-terser");
const config = require("./config.json");
const ngAnnotate = require("gulp-ng-annotate");
const minified = require("./plugins/preferminified")

const { injectSections, PRODUCTION } = require("./plugins/helpers");

const bundler = () =>
  merge(
    config.bundle.map(bundle =>
      merge(
        [
          src(bundle.inputFiles)
            .pipe(filter("**/*.js"))
            .pipe(gulpif(PRODUCTION, minified()))
            .pipe(gulpif(PRODUCTION, cache(ngAnnotate())))
            .pipe(gulpif(PRODUCTION, cache(terser({ mangle: true, compress: true }))))

            .pipe(concat(bundle.name + ".js"))
            .pipe(gulpif(PRODUCTION, rev()))
            .pipe(dest(join(config.dist, "js"))),

          src(bundle.inputFiles)
            .pipe(filter(["**/*.css", "**/*.less"]))
            .pipe(cache(less()))
            .pipe(concat(bundle.name + ".css"))
            .pipe(gulpif(PRODUCTION, rev()))
            .pipe(dest(join(config.dist, "css"))),

          bundle.assets == null
            ? null
            : merge(
                bundle.assets.map(asset =>
                  src(asset.glob).pipe(dest(join(config.dist, asset.prefix)))
                )
              )
        ].filter(x => x != null)
      )
    )
  );

const inject = () =>
  injectSections(src(config.injectTargets), config.dist).pipe(
    dest(f => f.base)
  );

module.exports = {
  default: series(bundler, inject),
  inject,
  bundler
};
