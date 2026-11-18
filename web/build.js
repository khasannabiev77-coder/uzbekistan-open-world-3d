// Builds downloadable, offline-ready files from the *.body.html sources.
// Usage: node build.js <path-to-three.min.js r128>
const fs = require('fs'), path = require('path');
const here = __dirname, out = path.join(here, '..', 'Yuklab_olish');
fs.mkdirSync(out, { recursive: true });
const threePath = process.argv[2];
const three = threePath ? fs.readFileSync(threePath, 'utf8') : null;
if (three && three.includes('</script')) throw new Error('three.min.js contains </script');

function wrap(body, lang) {
  const i = body.indexOf('</style>') + '</style>'.length;
  return `<!doctype html>\n<html lang="${lang}">\n<head>\n<meta charset="utf-8">\n<meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover">\n<meta name="theme-color" content="#061013">\n${body.slice(0, i)}\n</head>\n<body>\n${body.slice(i)}\n</body>\n</html>\n`;
}
const strip = s => s.replace(/^window\.UZ=.*\n/m, '');

let game = strip(fs.readFileSync(path.join(here, 'game.body.html'), 'utf8'));
fs.writeFileSync(path.join(here, 'game.publish.html'), game);
if (three) game = game.replace(/<script src="https:\/\/cdnjs\.cloudflare\.com\/ajax\/libs\/three\.js\/r128\/three\.min\.js"><\/script>/, () => '<script>/* three.js r128 — MIT License — https://threejs.org */\n' + three + '\n</script>');
fs.writeFileSync(path.join(out, 'UzbekistanOpenWorld3D.html'), wrap(game, 'uz'));
fs.writeFileSync(path.join(out, 'Reklama.html'), wrap(fs.readFileSync(path.join(here, 'ad.body.html'), 'utf8'), 'uz'));
for (const f of ['UzbekistanOpenWorld3D.html', 'Reklama.html']) console.log(f, (fs.statSync(path.join(out, f)).size / 1024).toFixed(0) + ' KB');
