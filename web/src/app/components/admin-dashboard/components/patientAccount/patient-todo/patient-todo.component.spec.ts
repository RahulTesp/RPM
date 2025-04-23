import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientTodoComponent } from './patient-todo.component';

describe('PatientTodoComponent', () => {
  let component: PatientTodoComponent;
  let fixture: ComponentFixture<PatientTodoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PatientTodoComponent ]
    })
    .compileComponents();
  //});

  //beforeEach(() => {
    fixture = TestBed.createComponent(PatientTodoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
