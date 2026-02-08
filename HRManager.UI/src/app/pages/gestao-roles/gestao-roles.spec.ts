import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoRoles } from './gestao-roles';

describe('GestaoRoles', () => {
  let component: GestaoRoles;
  let fixture: ComponentFixture<GestaoRoles>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoRoles]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoRoles);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
