// apt install libc-dev -yqq
// gcc go.c -o go -lpthread; ./go

#include "pthread.h"
// #include <tls.h>
#include <stdio.h>

int main() {
  pthread_t id;
  printf("pthread_t size: %lu bytes\n", sizeof(id));
  id=pthread_self();
  printf("thread id: %lu\n", id);
}

