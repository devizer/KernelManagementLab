
export class ProcessRowsFilters {
    
    // static PredefinedTop = [10, 30, 100, 0];
    
    TopFilter = 0; // all processes
    NeedNoFilter = true;        // Any 
    NeedKernelThreads = false;  // Kernel
    NeedServices = false;       // Services
    NeedContainers = false;     // Containers
    
    static getDefault() {
        const ret = new ProcessRowsFilters();
        ret.TopFilter = 12;
        return ret;
    }
}
