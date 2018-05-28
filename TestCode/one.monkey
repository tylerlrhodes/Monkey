
let a = 10

let test = fn(x) {
  if (x == 0)
  {
    return f()
  }
  let x = x - 1
  return test (x)
}

test(a)

let f = fn() { return 1 }

let result = test(a)

puts(result)


