/**
 * Normalises Windows drive-letter casing in Node's module-resolution cache so that
 * C:\path and c:\path always resolve to the same module instance.  Without this,
 * tsx (running in CWD "c:\...") and the Cucumber runner (using "C:\...") each get
 * their own copy of supportCodeLibraryBuilder, and setWorldConstructor fires on the
 * un-reset copy → "status: PENDING" crash.
 */
const Module = require("module");
const path   = require("path");

const _orig = Module._resolveFilename.bind(Module);
Module._resolveFilename = function (request, parent, isMain, options) {
  const resolved = _orig(request, parent, isMain, options);
  // On Windows, normalise the drive letter to uppercase so the cache key is consistent.
  return resolved.replace(/^[a-zA-Z]:/, (d) => d.toUpperCase());
};

// Hand off to the real Cucumber CLI.
require(path.join(path.dirname(__dirname), "node_modules", "@cucumber", "cucumber", "bin", "cucumber.js"));
