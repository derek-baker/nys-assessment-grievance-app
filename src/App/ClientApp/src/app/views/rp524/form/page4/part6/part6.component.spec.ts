import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { Part6Component } from './part6.component';

describe('Part6Component', () => {
  let component: Part6Component;
  let fixture: ComponentFixture<Part6Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ Part6Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(Part6Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
