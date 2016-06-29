var express = require('express');
var multer = require('multer');
var fs = require('fs');
var path = require('path');
var os = require('os');
var app = express();
var html=require('html');
var UPLOAD_PATH = "C://SmartStor";
var SERVER_PORT = 3000;
var ip=require('ip');
var request=require('request');
const userInfo = require('user-info');

function printRequestHeaders(req) {
    console.log("\nReceived headers");
    console.log("----------------");

    for (var key in req.headers) {
        console.log(key + ": " + req.headers[key]);
    }

    console.log("");
}

function printRequestParameters(req) {
    console.log("\nReceived Parameters");
    console.log("-------------------");

    for (var key in req.body) {
        console.log(key + ": " + req.body[key]);
    }

    if (Object.keys(req.body).length === 0)
        console.log("no text parameters\n");
    else
        console.log("");
}

function getEndpoints(ipAddress) {
    return "http://" + ipAddress + ":" + SERVER_PORT + "/upload/multipart"
}

function printAvailableEndpoints() {
    var ifaces = os.networkInterfaces();

    Object.keys(ifaces).forEach(function (ifname) {
        ifaces[ifname].forEach(function (iface) {
            // skip internal (i.e. 127.0.0.1) and non-ipv4 addresses
            if ('IPv4' !== iface.family || iface.internal !== false) {
                return;
            }

            console.log(getEndpoints(iface.address));
        });
    });
}

var multipartReqInterceptor = function(req, res, next) {
    console.log("\n\nHTTP/Multipart Upload Request from: " + req.ip);
    printRequestHeaders(req);

    next();
};

// configure multer for upload management
var fileUploadCompleted = false;
var multerFiles = multer({
    dest: UPLOAD_PATH,
    rename: function (fieldname, filename) {
        return filename;
    },

    onParseEnd: function(req, next) {
        printRequestParameters(req);

        next();
    },

    onFileUploadStart: function (file) {
        console.log("Started file upload\n  parameter name: " +
                    file.fieldname + "\n  file name: " +
                    file.originalname + "\n  mime type: " + file.mimetype);
    },

    onFileUploadComplete: function (file) {
        var fullPath = path.resolve(UPLOAD_PATH, file.originalname);
        console.log("Completed file upload\n  parameter name: " +
                    file.fieldname + "\n  file name: " +
                    file.originalname + "\n  mime type: " + file.mimetype +
                    "\n  in: " + fullPath);
        fileUploadCompleted = true;
    }
});



var multipartUploadHandler = function(req, res) {
    if (fileUploadCompleted) {
        fileUploadCompleted = false;
        res.header('transfer-encoding', ''); // disable chunked transfer encoding
        res.end("Upload Ok!");
    }
};
var _ = require('lodash');
var express = require('express');
var fs = require('fs');
var path = require('path');
var util = require('util');

var dir =  'C:\\SmartStor'
app.use(express.static(dir)); //app public directory
app.use(express.static(__dirname)); //module directory
app.use(express.static(path.join(__dirname, 'public')));
app.set('views', __dirname + '/views');
app.set('view engine', 'ejs');
app.engine('.html', require('ejs').renderFile);

var program = require('commander');

program.port= SERVER_PORT;
app.get('/', function(req, res) {
    var url = ip.address +"get-file";
    request({
        url: url,
        json: true
    },function (error,response,body) {

        console.log("got request from " + req.ip)
        var currentDir = dir;
        var query = req.query.path || '';
        if (query) currentDir = path.join(dir, query);
        console.log("browsing ", currentDir);
        fs.readdir(currentDir, function (err, files) {
            if (err) {
                throw err;
            }
            var data = [];
            files
                .filter(function (file) {
                    return true;
                }).forEach(function (file) {
                try {
                    //console.log("processing ", file);
                    var isDirectory = fs.statSync(path.join(currentDir, file)).isDirectory();
                    if (isDirectory) {
                        data.push({Name: file, IsDirectory: true, Path: path.join(query, file)});
                    } else {
                        var ext = path.extname(file);
                        if (program.exclude && _.includes(program.exclude, ext)) {
                            console.log("excluding file ", file);
                            return;
                        }
                        data.push({Name: file, Ext: ext, IsDirectory: false, Path: path.join(query, file)});
                    }

                } catch (e) {
                    console.log(e);
                }

            });
            data = _.sortBy(data, function (f) {
                return f.Name
            });
            res.json(data);
        });
    });
});


// handle multipart uploads
app.post('/upload/multipart', multipartReqInterceptor, multerFiles, multipartUploadHandler);
app.get('/get-info',function (req,res)
{
	info=[];
    console.log("request for info received from"+ req.ip);
    console.log("sending address" + ip.address().toString());
	info.push({ip:ip.address().toString(),devicename:userInfo().username.toString()+","+os.type().toString()});
	res.json(info);
    


})



var server = app.listen(SERVER_PORT, function() {
	
    console.log("Web server started. Listening on all interfaces on port " +
                server.address().port);

    console.log("\nThe following endpoints are available for upload testing:\n");
    printAvailableEndpoints();

});
