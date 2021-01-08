#ifndef LLVM_CRAP_H
#define LLVM_CRAP_H
// *****************************************************************************
// llvm-crap.h                                                        XL project
// *****************************************************************************
//
// File description:
//
//     LLVM Compatibility Recovery Adaptive Protocol
//
//     LLVM keeps breaking the API from release to release.
//     Of course, none of the choices are documented anywhere, and
//     the documentation has been out of date for years.
//
//     See llvm-crap as an attempt at reverse engineering all the API
//     changes over time to be able to compile with various versions of LLVM
//     Be prepared for the worst. It's so ugly I had to put it in its own file
//
// *****************************************************************************

#ifndef LLVM_VERSION
#error "Sorry, no can do anything without knowing the LLVM version"
#elif LLVM_VERSION < 370
// At some point, I have only so much time to waste on this.
// Feel free to enhance if you care about earlier versions of LLVM.
#error "LLVM 3.6 and earlier are not supported in this code."
#endif

#define LLVM_CRAP_DIAPER_OPEN
#include "llvm-crap.h"

// Fortunately, these LLVM headers are sufficient for our interface,
// and remained somewhat consistent across versions. Lucky us!
#include <llvm/IR/Type.h>
#include <llvm/IR/Constant.h>
#include <llvm/IR/DerivedTypes.h>
#include <llvm/IR/Function.h>

#include <recorder/recorder.h>
#include <string>

#define LLVM_CRAP_DIAPER_CLOSE
#include "llvm-crap.h"
