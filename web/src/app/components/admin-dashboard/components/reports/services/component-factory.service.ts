import {
  ComponentFactoryResolver,
  Injectable,
  Type,
  ViewContainerRef,
} from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ComponentFactoryService {
  constructor(private componentFactoryResolver: ComponentFactoryResolver) {}
  loadComponent<T>(viewContainerRef: ViewContainerRef, component: Type<T>): T {
    // Clear existing components
    viewContainerRef.clear();

    // Create component factory
    const componentFactory =
      this.componentFactoryResolver.resolveComponentFactory(component);

    // Create component
    const componentRef = viewContainerRef.createComponent(componentFactory);

    // Return component instance
    return componentRef.instance;
  }
}
