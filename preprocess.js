var fs = require('fs');
var path = require('path');
const marked = require('marked');

// var tmpdir = __dirname + './tmp/';
var tmpdir = './tmp/';

if (!fs.existsSync(tmpdir)){
    fs.mkdirSync(tmpdir);
}

function ensureExists(path, mask, cb) {
    if (typeof mask == 'function') { // allow the `mask` parameter to be optional
        cb = mask;
        mask = 0777;
    }
    fs.mkdir(path, mask, function(err) {
        if (err) {
            if (err.code == 'EEXIST') cb(null); // ignore the error if the folder already exists
            else cb(err); // something else went wrong
        } else cb(null); // successfully created folder
    });
}

function readFiles(dirname, onFileContent, onError) {
  fs.readdir(dirname, function(err, filenames) {
    if (err) {
      onError(err);
      return;
    }
    filenames.forEach(function(filename) {
      var stat = fs.statSync(dirname + filename);
      if (stat && stat.isDirectory()) { 
      }else{
        fs.readFile(dirname + filename, 'utf-8', function(err, content) {
          if (err) {
            onError(err);
            return;
          }
          onFileContent(dirname + filename, content);
        });
      }
    });
  });
}

function copyFileSync( source, target ) {

    var targetFile = target;

    //if target is a directory a new file with the same name will be created
    if ( fs.existsSync( target ) ) {
        if ( fs.lstatSync( target ).isDirectory() ) {
            targetFile = path.join( target, path.basename( source ) );
        }
    }

    fs.writeFileSync(targetFile, fs.readFileSync(source));
}

function copyFolderRecursiveSync( source, target ) {
    var files = [];

    //check if folder needs to be created or integrated
    var targetFolder = path.join( target, path.basename( source ) );
    if ( !fs.existsSync( targetFolder ) ) {
        fs.mkdirSync( targetFolder );
    }

    //copy
    if ( fs.lstatSync( source ).isDirectory() ) {
        files = fs.readdirSync( source );
        files.forEach( function ( file ) {
            var curSource = path.join( source, file );
            if ( fs.lstatSync( curSource ).isDirectory() ) {
                copyFolderRecursiveSync( curSource, targetFolder );
            } else {
                copyFileSync( curSource, targetFolder );
            }
        } );
    }
}





var glossary = [
  { 
    name: '哈希',
    detail: 'Hash',
    ref: '#/section1/1-Overview'
  },
  { 
    name: '协因子',
    detail: 'cofactor',
  },
  { 
    name: '未使用交易',
    detail: 'UTXO: Unspent Transaction(TX) Output',
  },
  { 
    name: '公钥哈希值支付',
    detail: 'P2PKH: Pay To Public Key Hash',
  },
];

function preprocess(content) {
  let pcnt = content
    .replace(
    /<!-- code:(.*?)(?:;branch:(.*?))?(?:;line:(\d+?)-(\d+?))? -->/g,
    function(match, path, branch, lstart, lend){
      branch = branch || "master";
      const loc = (lstart && lend) ? `#L${lstart}-L${lend}` : "";
      const locDesc = (lstart && lend) ? `\n代码行：${lstart}-${lend}` : "";
      return `<p class="coderef">
  参考代码：
  <a href="https://github.com/uchaindb/LearnBlockchainByCode/blob/${branch}/${path}${loc}"
      title="分支：\[${branch}\]${locDesc}" target="_blank">
      ${path}
  </a>
</p>`;
    })
    .replace(/读书提示：.*?。/g, "");

  for (var i = 0, len = glossary.length; i < len; i++) {
    var re = new RegExp('`' + glossary[i].name + '`', 'g');
    pcnt = pcnt.replace(re, `<abbr title="${glossary[i].detail}">${glossary[i].name}</abbr>`);
  }
  return pcnt;
}

readFiles('section1/', function(filename, content) {
  let pcnt = preprocess(content);
  let pcnt = marked(pcnt);
  let filepath = tmpdir + filename;
  // console.log(filepath);

  ensureExists(path.dirname(filepath), 0777, function(err) {
    if (err) 
      console.log(err);
    else {
      fs.writeFile(filepath, pcnt, function(err) {
        if(err) { return console.log(err); }
        console.log("The file [" + filename + "] was saved!");
      });
    }
  });
}, function(err) {
  throw err;
});

fs.copyFileSync('Prologue.md', tmpdir + 'Prologue.md');
fs.copyFileSync('README.md', tmpdir + 'README.md');
copyFolderRecursiveSync('section1/_images', tmpdir);
