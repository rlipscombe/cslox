function it() {
    var n = 0;

    function inc() {
        n = n + 1;
    }

    function get() {
        return n;
    }

    return [inc, get];
}

[ai, ag] = it();
[bi, bg] = it();

ai();
ai();
console.log(ag());
console.log(bg());

function other() {
    function get() {
        return x;
    }

    return get;
}

var x = 42;
var g = other();
x = 101;

console.log(g());

var yy = 123;

function again() {
    return yy;
}

var h = again;

var yy = 321;

console.log(h());
