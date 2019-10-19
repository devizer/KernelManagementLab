#include <stdio.h>
#include <mach/mach_init.h>
#include <mach/thread_act.h>
#include <mach/mach_port.h>

// https://stackoverflow.com/questions/13893134/get-current-pthread-cpu-usage-mac-os-x
// https://developer.apple.com/documentation/kernel/1418630-thread_info?language=objc
// p/invoke: mach_thread_self, thread_info, mach_port_deallocate


int main() {
  mach_port_t thread;
  kern_return_t kr;
  mach_msg_type_number_t count;
  thread_basic_info_data_t info;
  thread_flavor_t flavor = THREAD_BASIC_INFO;

  // https://opensource.apple.com/source/xnu/xnu-792/osfmk/mach/thread_info.h.auto.html
  int xx=42; printf("size of int: %lu\n", sizeof(xx));
  printf("sizeof mach_port_t thread: %lu bytes\n", sizeof(thread));
  printf("kern_return_t kr: %lu bytes\n", sizeof(kr));
  printf("mach_msg_type_number_t count: %lu bytes\n", sizeof(count));
  printf("thread_basic_info_data_t info: %lu bytes\n", sizeof(info));
  printf("constant THREAD_BASIC_INFO_COUNT: %d\n", THREAD_BASIC_INFO_COUNT);
  printf("constant THREAD_BASIC_INFO (%lu bytes): %d\n", sizeof(flavor), THREAD_BASIC_INFO);
  printf("constant KERN_SUCCESS: %d\n", KERN_SUCCESS);
  printf("constant TH_FLAGS_IDLE: %d\n", TH_FLAGS_IDLE);

  thread = mach_thread_self();
  printf("thread [mach_thread_self()]: %d\n", thread);


  count = THREAD_BASIC_INFO_COUNT;
  kr = thread_info(thread, THREAD_BASIC_INFO, (thread_info_t) &info, &count);

  if (kr == KERN_SUCCESS && (info.flags & TH_FLAGS_IDLE) == 0) {
      // usage->utime.tv_sec  = info.user_time.seconds;
      // usage->utime.tv_usec = info.user_time.microseconds;
      // usage->stime.tv_sec  = info.system_time.seconds;
      // usage->stime.tv_usec = info.system_time.microseconds;
      printf("info.flags (%lu bytes): %d\n", sizeof(info.flags), info.flags);
      printf("info.user_time.seconds (%lu bytes): %d\n", sizeof(info.user_time.seconds), info.user_time.seconds);
      printf("info.user_time.microseconds (%lu bytes): %d\n", sizeof(info.user_time.microseconds), info.user_time.microseconds);
      printf("info.system_time.seconds (%lu bytes): %d\n", sizeof(info.system_time.seconds), info.system_time.seconds);
      printf("info.system_time.microseconds (%lu bytes): %d\n", sizeof(info.system_time.microseconds), info.system_time.microseconds);

      printf("size of thread_basic_info.user_time: %lu bytes\n", sizeof(info.user_time));
      printf("size of thread_basic_info.system_time: %lu bytes\n", sizeof(info.system_time));
      printf("size of thread_basic_info.cpu_usage: %lu bytes\n", sizeof(info.cpu_usage));
      printf("size of thread_basic_info.policy: %lu bytes\n", sizeof(info.policy));
      printf("size of thread_basic_info.run_state: %lu bytes\n", sizeof(info.run_state));
      printf("size of thread_basic_info.flags: %lu bytes\n", sizeof(info.flags));
      printf("size of thread_basic_info.suspend_count: %lu bytes\n", sizeof(info.suspend_count));
      printf("size of thread_basic_info.sleep_time: %lu bytes\n", sizeof(info.sleep_time));
  }
  else {
      // should not happen
      printf("Could not retreive thread info.");
      // bzero(usage, sizeof(struct usage));
  }

  
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  printf("mach_thread_self(): %d\n", mach_task_self());
  int kResult = mach_port_deallocate(mach_task_self(), thread);
}


/*
struct thread_basic_info {
        time_value_t    user_time;      // user run time 
        time_value_t    system_time;    // system run time 
        integer_t       cpu_usage;      // scaled cpu usage percentage 
        policy_t        policy;         // scheduling policy in effect 
        integer_t       run_state;      // run state (see below) 
        integer_t       flags;          // various flags (see below) 
        integer_t       suspend_count;  // suspend count for thread 
        integer_t       sleep_time;     // number of seconds that thread has been sleeping 
}; 
*/

/*
size of int: 4
sizeof mach_port_t thread: 4 bytes
kern_return_t kr: 4 bytes
mach_msg_type_number_t count: 4 bytes
thread_basic_info_data_t info: 40 bytes
constant THREAD_BASIC_INFO_COUNT: 10
constant THREAD_BASIC_INFO: 3
constant KERN_SUCCESS: 0
constant TH_FLAGS_IDLE: 2

info.user_time.seconds (4 bytes): 0
info.user_time.microseconds (4 bytes): 697
info.system_time.seconds (4 bytes): 0
info.system_time.microseconds (4 bytes): 1505

size of thread_basic_info.user_time: 8 bytes
size of thread_basic_info.system_time: 8 bytes
size of thread_basic_info.cpu_usage: 4 bytes
size of thread_basic_info.policy: 4 bytes
size of thread_basic_info.run_state: 4 bytes
size of thread_basic_info.flags: 4 bytes
size of thread_basic_info.suspend_count: 4 bytes
size of thread_basic_info.sleep_time: 4 bytes
*/
